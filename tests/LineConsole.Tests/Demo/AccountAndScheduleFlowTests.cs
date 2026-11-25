using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LineConsole.Application.RichMenus;
using LineConsole.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LineConsole.Tests.Demo;

/// <summary>
/// M2 的驗收：一個新使用者從零開始，走完「新增官方帳號 → 建立圖文選單 → 排程 → 時間到自動切換 → 自動還原」。
/// 整個 API 都是真的（展示模式：SQLite＋模擬 LINE），只有「時間」由測試直接叫排程執行器指定。
/// </summary>
public sealed class AccountAndScheduleFlowTests : IClassFixture<AccountAndScheduleFlowTests.App>
{
    public sealed class App : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder) =>
            builder.UseSetting("LineConsole:Mode", "Demo").UseSetting("LineConsole:Scheduler:Enabled", "false");
    }

    private readonly App _app;
    private readonly HttpClient _client;
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public AccountAndScheduleFlowTests(App app)
    {
        _app = app;
        _client = app.CreateClient();
    }

    private async Task<string> NewUserAsync()
    {
        var email = $"owner-{Guid.NewGuid():N}@example.com";
        (await _client.PostAsJsonAsync("/api/auth/register", new { email, password = "Passw0rd", displayName = "新店家" }, Ct)).EnsureSuccessStatusCode();
        var login = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = "Passw0rd" }, Ct);
        return (await login.Content.ReadFromJsonAsync<JsonElement>(Ct)).GetProperty("data").GetProperty("token").GetString()!;
    }

    private async Task<(HttpStatusCode Status, JsonElement Body)> SendAsync(HttpMethod method, string url, string token, object? json = null, HttpContent? content = null)
    {
        var req = new HttpRequestMessage(method, url) { Content = content ?? (json is null ? null : JsonContent.Create(json)) };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var res = await _client.SendAsync(req, Ct);
        var text = await res.Content.ReadAsStringAsync(Ct);
        return (res.StatusCode, text.Length == 0 ? default : JsonDocument.Parse(text).RootElement.Clone());
    }

    private static MultipartFormDataContent MenuForm(string name)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent("false"), "Selected" },
            { new StringContent(name), "Name" },
            { new StringContent("點我"), "ChatBarText" },
            { new StringContent("2500"), "Size.Width" },
            { new StringContent("843"), "Size.Height" },
            { new StringContent("0"), "Areas[0].Bounds.X" },
            { new StringContent("0"), "Areas[0].Bounds.Y" },
            { new StringContent("2500"), "Areas[0].Bounds.Width" },
            { new StringContent("843"), "Areas[0].Bounds.Height" },
            { new StringContent("message"), "Areas[0].Action.Type" },
            { new StringContent("你好"), "Areas[0].Action.Text" },
        };
        var image = new ByteArrayContent([0xFF, 0xD8, 0xFF, 0xD9]);
        image.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        form.Add(image, "Image", "menu.jpg");
        return form;
    }

    [Fact]
    public async Task 新店家從零開始_新增帳號_建立選單_排程_時間到自動切換再還原()
    {
        var token = await NewUserAsync();

        // 1. 新使用者還沒有官方帳號（註冊不再需要先填 LINE 憑證）
        var (_, list) = await SendAsync(HttpMethod.Get, "/api/accounts", token);
        Assert.Equal(0, list.GetProperty("data").GetArrayLength());

        // 2. 新增官方帳號：token 會先送去 LINE 驗證
        var (bad, badBody) = await SendAsync(HttpMethod.Post, "/api/accounts", token, new { channelId = "1", channelSecret = "s", channelAccessToken = "invalid-xyz" });
        Assert.Equal(HttpStatusCode.BadRequest, bad);
        Assert.Equal("LINE_TOKEN_INVALID", badBody.GetProperty("error").GetProperty("code").GetString());

        var (ok, created) = await SendAsync(HttpMethod.Post, "/api/accounts", token, new { channelName = "新店家", channelId = "2001", channelSecret = "my-secret", channelAccessToken = "my-real-token" });
        Assert.Equal(HttpStatusCode.OK, ok);
        var account = created.GetProperty("data");
        var accountId = account.GetProperty("id").GetString();
        Assert.StartsWith("@", account.GetProperty("basicId").GetString());
        Assert.DoesNotContain("my-real-token", created.GetRawText());   // token、secret 只進不出

        var (dup, dupBody) = await SendAsync(HttpMethod.Post, "/api/accounts", token, new { channelId = "2001", channelSecret = "s", channelAccessToken = "my-real-token" });
        Assert.Equal(HttpStatusCode.BadRequest, dup);
        Assert.Equal("ACCOUNT_DUPLICATED", dupBody.GetProperty("error").GetProperty("code").GetString());

        // 憑證在資料庫裡是加密的
        using (var scope = _app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var row = await db.LineOfficialAccounts.AsNoTracking().SingleAsync(a => a.Id == Guid.Parse(accountId!), Ct);
            Assert.NotEqual("my-real-token", row.ChannelAccessToken);
            Assert.NotEqual("my-secret", row.ChannelSecret);
        }

        // 3. 建立圖文選單
        var (createStatus, menuBody) = await SendAsync(HttpMethod.Post, $"/api/accounts/{accountId}/richmenu/with-image", token, content: MenuForm("聖誕特賣"));
        Assert.Equal(HttpStatusCode.OK, createStatus);
        var menuId = menuBody.GetProperty("data").GetProperty("richMenuId").GetString();

        var (_, noDefault) = await SendAsync(HttpMethod.Get, $"/api/accounts/{accountId}/richmenu/default", token);
        Assert.Equal(JsonValueKind.Null, noDefault.GetProperty("data").ValueKind);   // 沒有預設選單：200＋null，不是錯誤

        // 4. 排程：一分鐘後開始、兩分鐘後結束；同一時段不能再排
        var now = DateTimeOffset.UtcNow;
        var (schedStatus, sched) = await SendAsync(HttpMethod.Post, $"/api/accounts/{accountId}/richmenu/schedules", token,
            new { richMenuId = menuId, startTime = now.AddMinutes(1), endTime = now.AddMinutes(2) });
        Assert.Equal(HttpStatusCode.OK, schedStatus);
        Assert.Equal("Pending", sched.GetProperty("data").GetProperty("status").GetString());

        var (overlap, overlapBody) = await SendAsync(HttpMethod.Post, $"/api/accounts/{accountId}/richmenu/schedules", token,
            new { richMenuId = menuId, startTime = now.AddMinutes(1.5), endTime = now.AddMinutes(5) });
        Assert.Equal(HttpStatusCode.BadRequest, overlap);
        Assert.Equal("SCHEDULE_OVERLAP", overlapBody.GetProperty("error").GetProperty("code").GetString());

        // 5. 時間到：背景服務每次呼叫的就是這個執行器
        await RunSchedulerAt(now.AddMinutes(1.5));
        var (_, during) = await SendAsync(HttpMethod.Get, $"/api/accounts/{accountId}/richmenu/default", token);
        Assert.Equal(menuId, during.GetProperty("data").GetProperty("richMenuId").GetString());

        // 6. 結束：還原成排程前的狀態（原本沒有預設 → 清掉）
        await RunSchedulerAt(now.AddMinutes(3));
        var (_, after) = await SendAsync(HttpMethod.Get, $"/api/accounts/{accountId}/richmenu/default", token);
        Assert.Equal(JsonValueKind.Null, after.GetProperty("data").ValueKind);

        var (_, schedules) = await SendAsync(HttpMethod.Get, $"/api/accounts/{accountId}/richmenu/schedules", token);
        Assert.Equal("Completed", schedules.GetProperty("data")[0].GetProperty("status").GetString());

        // 7. 別人看不到、刪不掉
        var stranger = await NewUserAsync();
        Assert.Equal(HttpStatusCode.NotFound, (await SendAsync(HttpMethod.Get, $"/api/accounts/{accountId}/richmenu/schedules", stranger)).Status);
        Assert.Equal(HttpStatusCode.NotFound, (await SendAsync(HttpMethod.Delete, $"/api/accounts/{accountId}", stranger)).Status);

        // 8. 解除綁定：帳號和它的本地資料都清掉，之後再操作就是 404
        Assert.Equal(HttpStatusCode.OK, (await SendAsync(HttpMethod.Delete, $"/api/accounts/{accountId}", token)).Status);
        var (_, empty) = await SendAsync(HttpMethod.Get, "/api/accounts", token);
        Assert.Equal(0, empty.GetProperty("data").GetArrayLength());
        Assert.Equal(HttpStatusCode.NotFound, (await SendAsync(HttpMethod.Get, $"/api/accounts/{accountId}/richmenu/list", token)).Status);
    }

    [Fact]
    public async Task 刪除圖文選單_本地紀錄也一起刪掉()
    {
        var token = await NewUserAsync();
        var (_, created) = await SendAsync(HttpMethod.Post, "/api/accounts", token, new { channelId = "3001", channelSecret = "s", channelAccessToken = "tok-" + Guid.NewGuid() });
        var accountId = Guid.Parse(created.GetProperty("data").GetProperty("id").GetString()!);
        var (_, menu) = await SendAsync(HttpMethod.Post, $"/api/accounts/{accountId}/richmenu/with-image", token, content: MenuForm("要刪的選單"));
        var lineId = menu.GetProperty("data").GetProperty("richMenuId").GetString();

        async Task<int> LocalCount()
        {
            using var scope = _app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return await db.RichMenus.CountAsync(m => m.LineOfficialAccountId == accountId && m.LineRichMenuId == lineId, Ct);
        }

        Assert.Equal(1, await LocalCount());
        Assert.Equal(HttpStatusCode.OK, (await SendAsync(HttpMethod.Delete, $"/api/accounts/{accountId}/richmenu/{lineId}", token)).Status);
        Assert.Equal(0, await LocalCount());

        // 已經不存在的選單：404，不是 500
        Assert.Equal(HttpStatusCode.NotFound, (await SendAsync(HttpMethod.Get, $"/api/accounts/{accountId}/richmenu/{lineId}", token)).Status);
        Assert.Equal(HttpStatusCode.NotFound, (await SendAsync(HttpMethod.Delete, $"/api/accounts/{accountId}/richmenu/{lineId}", token)).Status);
    }

    private async Task RunSchedulerAt(DateTimeOffset time)
    {
        using var scope = _app.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<IRichMenuScheduleRunner>().RunDueAsync(time.UtcDateTime, Ct);
    }
}
