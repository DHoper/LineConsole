using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LineConsole.Tests.Demo;

/// <summary>
/// 展示模式的端到端測試：啟動整個 API（SQLite 記憶體資料庫＋模擬 LINE＋展示資料），用 HTTP 走一遍。
/// 這也是 M1「clone 下來、不需要任何帳號就能用」的驗收。
/// </summary>
public sealed class DemoModeTests : IClassFixture<DemoModeTests.DemoApp>
{
    private readonly HttpClient _client;

    public DemoModeTests(DemoApp app)
    {
        _client = app.CreateClient();
    }

    public sealed class DemoApp : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder) =>
            builder.UseSetting("LineConsole:Mode", "Demo")
                .UseSetting("LineConsole:Scheduler:Enabled", "false");   // 測試自己控制時間，不讓背景服務照真實時間跑
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private async Task<JsonElement> LoginAsync(string email, string password)
    {
        var res = await _client.PostAsJsonAsync("/api/auth/login", new { email, password }, Ct);
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<JsonElement>(Ct)).GetProperty("data");
    }

    private HttpRequestMessage Get(string url, string token)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return req;
    }

    [Fact]
    public async Task 系統資訊_告訴前端這是展示模式和展示帳號()
    {
        var body = await _client.GetFromJsonAsync<JsonElement>("/api/system/info", Ct);
        var data = body.GetProperty("data");
        Assert.Equal("Demo", data.GetProperty("mode").GetString());
        Assert.Equal("demo@lineconsole.dev", data.GetProperty("demoAccount").GetProperty("email").GetString());
    }

    [Fact]
    public async Task 展示帳號登入後_看得到兩個官方帳號和圖文選單()
    {
        var login = await LoginAsync("demo@lineconsole.dev", "Demo1234");
        var token = login.GetProperty("token").GetString()!;
        var accounts = login.GetProperty("user").GetProperty("lineAccounts").EnumerateArray().ToList();
        Assert.Equal(2, accounts.Count);

        var cafe = accounts.Single(a => a.GetProperty("channelName").GetString() == "LineConsole 咖啡館").GetProperty("id").GetString();
        var res = await _client.SendAsync(Get($"/api/accounts/{cafe}/richmenu/list-with-preview", token), Ct);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var menus = (await res.Content.ReadFromJsonAsync<JsonElement>(Ct)).GetProperty("data").EnumerateArray().ToList();
        Assert.Equal(3, menus.Count);
        Assert.All(menus, m => Assert.False(string.IsNullOrEmpty(m.GetProperty("imageBase64").GetString())));

        var def = await _client.SendAsync(Get($"/api/accounts/{cafe}/richmenu/default", token), Ct);
        Assert.Equal(HttpStatusCode.OK, def.StatusCode);
    }

    [Fact]
    public async Task 新註冊的使用者_拿不到展示帳號的官方帳號()
    {
        var demo = await LoginAsync("demo@lineconsole.dev", "Demo1234");
        var demoAccountId = demo.GetProperty("user").GetProperty("lineAccounts")[0].GetProperty("id").GetString();

        var email = $"mallory-{Guid.NewGuid():N}@example.com";
        var register = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "Passw0rd",
            displayName = "Mallory",
            lineAccount = new { channelName = "Mallory 的店", channelId = "999", channelSecret = "s", channelAccessToken = "mallory-token" }
        }, Ct);
        register.EnsureSuccessStatusCode();
        var mallory = await LoginAsync(email, "Passw0rd");

        var res = await _client.SendAsync(Get($"/api/accounts/{demoAccountId}/richmenu/list", mallory.GetProperty("token").GetString()!), Ct);
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task 沒帶Token_呼叫API回401()
    {
        var res = await _client.GetAsync($"/api/accounts/{Guid.NewGuid()}/richmenu/list", Ct);
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task 不存在的API回404_JSON而不是前端頁面()
    {
        var res = await _client.GetAsync("/api/not-a-real-endpoint", Ct);
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        Assert.Equal("application/json", res.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task 前端頁面不用登入()
    {
        // 有沒有建置前端（wwwroot）會影響是 200 還是 404，但一定不能是 401
        var res = await _client.GetAsync("/auth/login", Ct);
        Assert.NotEqual(HttpStatusCode.Unauthorized, res.StatusCode);
    }
}
