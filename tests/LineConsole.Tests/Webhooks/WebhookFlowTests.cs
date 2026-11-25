using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using LineConsole.Application.Webhooks;
using LineConsole.Domain.Entities;
using LineConsole.Infrastructure.Data;
using LineConsole.Infrastructure.Data.EfEntities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LineConsole.Tests.Webhooks;

/// <summary>
/// M3 的驗收：LINE Webhook 從 HTTP 進來 → 驗簽 → 去重 → 收件匣 → 事件處理 → 好友名單、儀表板。
/// 用真的 API（展示模式：SQLite＋模擬 LINE），請求內容照 LINE 的格式自己組、簽章自己算（不借用被測的程式）。
/// 背景服務關掉，由測試決定什麼時候處理收件匣；時間固定在「今天下午兩點（台北）」，每日統計才算得準。
/// </summary>
public sealed class WebhookFlowTests : IClassFixture<WebhookFlowTests.App>
{
    public sealed class FixedClock : TimeProvider
    {
        public DateTimeOffset Now { get; set; }
        public override DateTimeOffset GetUtcNow() => Now;
    }

    public sealed class App : WebApplicationFactory<Program>
    {
        public static readonly TimeZoneInfo Taipei = TimeZoneInfo.FindSystemTimeZoneById("Asia/Taipei");
        public static readonly DateOnly Today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Taipei));

        public FixedClock Clock { get; } = new() { Now = new DateTimeOffset(Local(Today, 14), TimeSpan.Zero) };

        /// <summary>台北某天某點 → UTC</summary>
        public static DateTime Local(DateOnly day, int hour, int minute = 0) =>
            TimeZoneInfo.ConvertTimeToUtc(day.ToDateTime(new TimeOnly(hour, minute)), Taipei);

        protected override void ConfigureWebHost(IWebHostBuilder builder) =>
            builder.UseSetting("LineConsole:Mode", "Demo")
                .UseSetting("LineConsole:Scheduler:Enabled", "false")
                .UseSetting("LineConsole:Webhook:WorkerEnabled", "false")
                .ConfigureTestServices(s => s.AddSingleton<TimeProvider>(Clock));
    }

    private readonly App _app;
    private readonly HttpClient _client;
    private static CancellationToken Ct => TestContext.Current.CancellationToken;
    private static DateOnly Today => App.Today;

    public WebhookFlowTests(App app)
    {
        _app = app;
        _client = app.CreateClient();
    }

    // ---------- 驗簽 ----------

    [Fact]
    public async Task Signed_webhook_is_accepted_and_processed_into_friends()
    {
        var (token, accountId, secret) = await NewAccountAsync();
        var u1 = UserId(); var u2 = UserId();
        var body = Payload(Event("follow", u1, App.Local(Today, 9)), Event("follow", u2, App.Local(Today, 10)), Event("message", u1, App.Local(Today, 11)));

        var (status, json) = await PostWebhookAsync(accountId, body, Sign(secret, body));
        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal(3, json.GetProperty("data").GetProperty("accepted").GetInt32());

        // 先存進收件匣就回 200：還沒處理前好友名單是空的
        Assert.Equal(0, (await GetDataAsync($"/api/accounts/{accountId}/friends", token)).GetProperty("total").GetInt32());

        await ProcessInboxAsync();
        var friends = await GetDataAsync($"/api/accounts/{accountId}/friends", token);
        Assert.Equal(2, friends.GetProperty("total").GetInt32());
        var first = friends.GetProperty("items").EnumerateArray().Single(x => x.GetProperty("lineUserId").GetString() == u1);
        Assert.Equal("Following", first.GetProperty("status").GetString());
        Assert.False(string.IsNullOrEmpty(first.GetProperty("displayName").GetString()));   // 向 LINE 查了個人資料
        Assert.Equal(JsonValueKind.String, first.GetProperty("lastMessageAt").ValueKind);
    }

    [Fact]
    public async Task Bad_or_missing_signature_is_rejected_with_401_and_nothing_is_stored()
    {
        var (_, accountId, secret) = await NewAccountAsync();
        var body = Payload(Event("follow", UserId(), App.Local(Today, 9)));

        // 用別的 secret 簽
        var (s1, j1) = await PostWebhookAsync(accountId, body, Sign("someone-else", body));
        Assert.Equal(HttpStatusCode.Unauthorized, s1);
        Assert.Equal("WEBHOOK_SIGNATURE_INVALID", j1.GetProperty("error").GetProperty("code").GetString());

        // 簽章是對的，但 body 被改過
        var tampered = body.Replace("\"follow\"", "\"unfollow\"");
        Assert.Equal(HttpStatusCode.Unauthorized, (await PostWebhookAsync(accountId, tampered, Sign(secret, body))).Status);

        // 沒有簽章
        Assert.Equal(HttpStatusCode.Unauthorized, (await PostWebhookAsync(accountId, body, null)).Status);

        Assert.Equal(0, await CountInboxAsync(accountId));
    }

    [Fact]
    public async Task Unknown_account_returns_404()
    {
        var body = Payload();
        Assert.Equal(HttpStatusCode.NotFound, (await PostWebhookAsync(Guid.NewGuid(), body, Sign("x", body))).Status);
    }

    [Fact]
    public async Task Verify_request_with_empty_events_returns_200()
    {
        var (_, accountId, secret) = await NewAccountAsync();
        var body = Payload();   // LINE Developers 按「Verify」送的就是空的 events
        Assert.Equal(HttpStatusCode.OK, (await PostWebhookAsync(accountId, body, Sign(secret, body))).Status);
    }

    // ---------- 去重 ----------

    [Fact]
    public async Task Redelivered_event_is_stored_once_and_counted_once()
    {
        var (token, accountId, secret) = await NewAccountAsync();
        var e = Event("follow", UserId(), App.Local(Today, 9));
        var body = Payload(e);
        var (_, first) = await PostWebhookAsync(accountId, body, Sign(secret, body));
        Assert.Equal(1, first.GetProperty("data").GetProperty("accepted").GetInt32());

        // LINE 沒收到 200（逾時）就會重送：同一個 webhookEventId，標成 isRedelivery
        e["deliveryContext"]!["isRedelivery"] = true;
        var again = Payload(e);
        var (status, second) = await PostWebhookAsync(accountId, again, Sign(secret, again));
        Assert.Equal(HttpStatusCode.OK, status);   // 重送一樣回 200，不然 LINE 會一直重送
        Assert.Equal(0, second.GetProperty("data").GetProperty("accepted").GetInt32());
        Assert.Equal(1, second.GetProperty("data").GetProperty("duplicates").GetInt32());

        await ProcessInboxAsync();
        Assert.Equal(1, await CountInboxAsync(accountId));
        var dashboard = await GetDataAsync($"/api/accounts/{accountId}/dashboard", token);
        Assert.Equal(1, dashboard.GetProperty("following").GetInt32());
        Assert.Equal(1, dashboard.GetProperty("followsToday").GetInt32());
    }

    [Fact]
    public async Task Same_event_delivered_concurrently_is_stored_once()
    {
        var (_, accountId, secret) = await NewAccountAsync();
        var body = Payload(Event("follow", UserId(), App.Local(Today, 9)));

        var results = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => PostWebhookAsync(accountId, body, Sign(secret, body))));

        Assert.All(results, r => Assert.Equal(HttpStatusCode.OK, r.Status));
        Assert.Equal(1, results.Sum(r => r.Body.GetProperty("data").GetProperty("accepted").GetInt32()));
        Assert.Equal(7, results.Sum(r => r.Body.GetProperty("data").GetProperty("duplicates").GetInt32()));
        Assert.Equal(1, await CountInboxAsync(accountId));
    }

    [Fact]
    public async Task Database_unique_index_rejects_a_duplicate_event_id()
    {
        // 最後一道防線：就算程式的檢查被繞過，資料庫也不讓同一個帳號存兩筆相同的 webhookEventId
        var accountId = Guid.NewGuid();
        using var scope = _app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        LineWebhookEventEntity Row() => new()
        {
            Id = Guid.NewGuid(), AccountId = accountId, WebhookEventId = "01M89PGCWM33QNX40THP8FFSCE", EventType = "follow",
            OccurredAt = DateTime.UtcNow, Payload = "{}", ReceivedAt = DateTime.UtcNow
        };
        db.LineWebhookEvents.Add(Row());
        await db.SaveChangesAsync(Ct);
        db.LineWebhookEvents.Add(Row());
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync(Ct));
    }

    // ---------- 事件處理 ----------

    [Fact]
    public async Task Older_event_arriving_late_does_not_undo_a_newer_state()
    {
        var (token, accountId, secret) = await NewAccountAsync();
        var u = UserId();

        await SendAndProcessAsync(accountId, secret, Event("follow", u, App.Local(Today, 8)));
        await SendAndProcessAsync(accountId, secret, Event("unfollow", u, App.Local(Today, 11)));
        var late = Event("follow", u, App.Local(Today, 10));   // 10 點的 follow 在 11 點的 unfollow 之後才到
        await SendAndProcessAsync(accountId, secret, late);

        var friend = (await GetDataAsync($"/api/accounts/{accountId}/friends", token)).GetProperty("items")[0];
        Assert.Equal("Blocked", friend.GetProperty("status").GetString());
        Assert.Equal(LineWebhookEventStatus.Ignored, await InboxStatusAsync(accountId, (string)late["webhookEventId"]!));

        // 之後真的重新加入（時間比較新）就恢復
        await SendAndProcessAsync(accountId, secret, Event("follow", u, App.Local(Today, 12)));
        friend = (await GetDataAsync($"/api/accounts/{accountId}/friends", token)).GetProperty("items")[0];
        Assert.Equal("Following", friend.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Unsupported_events_are_marked_ignored()
    {
        var (token, accountId, secret) = await NewAccountAsync();
        var postback = Event("postback", UserId(), App.Local(Today, 9));
        var groupMessage = Event("message", null, App.Local(Today, 9));
        groupMessage["source"] = new JsonObject { ["type"] = "group", ["groupId"] = "C" + new string('1', 32) };

        await SendAndProcessAsync(accountId, secret, postback, groupMessage);

        Assert.Equal(LineWebhookEventStatus.Ignored, await InboxStatusAsync(accountId, (string)postback["webhookEventId"]!));
        Assert.Equal(LineWebhookEventStatus.Ignored, await InboxStatusAsync(accountId, (string)groupMessage["webhookEventId"]!));
        Assert.Equal(0, (await GetDataAsync($"/api/accounts/{accountId}/friends", token)).GetProperty("total").GetInt32());
    }

    // ---------- 好友名單、儀表板 ----------

    [Fact]
    public async Task Dashboard_counts_daily_follows_and_unfollows()
    {
        var (token, accountId, secret) = await NewAccountAsync();
        var yesterday = Today.AddDays(-1);
        string a = UserId(), b = UserId(), c = UserId();
        await SendAndProcessAsync(accountId, secret,
            Event("follow", a, App.Local(yesterday, 10)),
            Event("follow", b, App.Local(Today, 9)),
            Event("follow", c, App.Local(Today, 10)),
            Event("unfollow", b, App.Local(Today, 11)),
            Event("follow", c, App.Local(Today, 12)));   // 已經是好友又收到 follow：狀態沒變，不重複計算

        var d = await GetDataAsync($"/api/accounts/{accountId}/dashboard", token);
        Assert.Equal(2, d.GetProperty("following").GetInt32());
        Assert.Equal(1, d.GetProperty("blocked").GetInt32());
        Assert.Equal(2, d.GetProperty("followsToday").GetInt32());
        Assert.Equal(1, d.GetProperty("unfollowsToday").GetInt32());

        var daily = d.GetProperty("daily").EnumerateArray().ToArray();
        Assert.Equal(30, daily.Length);
        AssertDay(daily[^1], Today, follows: 2, unfollows: 1, following: 2);
        AssertDay(daily[^2], yesterday, follows: 1, unfollows: 0, following: 1);
        AssertDay(daily[^3], Today.AddDays(-2), follows: 0, unfollows: 0, following: 0);

        // 好友名單：篩選、分頁
        Assert.Equal(1, (await GetDataAsync($"/api/accounts/{accountId}/friends?status=blocked", token)).GetProperty("total").GetInt32());
        Assert.Equal(2, (await GetDataAsync($"/api/accounts/{accountId}/friends?status=following", token)).GetProperty("total").GetInt32());
        var page2 = await GetDataAsync($"/api/accounts/{accountId}/friends?page=2&pageSize=2", token);
        Assert.Equal(3, page2.GetProperty("total").GetInt32());
        Assert.Equal(1, page2.GetProperty("items").GetArrayLength());

        var name = (await GetDataAsync($"/api/accounts/{accountId}/friends?status=blocked", token)).GetProperty("items")[0].GetProperty("displayName").GetString()!;
        var search = await GetDataAsync($"/api/accounts/{accountId}/friends?keyword={Uri.EscapeDataString(name)}", token);
        Assert.Contains(search.GetProperty("items").EnumerateArray(), x => x.GetProperty("lineUserId").GetString() == b);
    }

    [Fact]
    public async Task Other_users_cannot_read_friends_or_dashboard()
    {
        var (_, accountId, _) = await NewAccountAsync();
        var (otherToken, _, _) = await NewAccountAsync();

        Assert.Equal(HttpStatusCode.NotFound, (await GetAsync($"/api/accounts/{accountId}/friends", otherToken)).Status);
        Assert.Equal(HttpStatusCode.NotFound, (await GetAsync($"/api/accounts/{accountId}/dashboard", otherToken)).Status);
        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.GetAsync($"/api/accounts/{accountId}/friends", Ct)).StatusCode);
    }

    // ---------- 展示模式 ----------

    [Fact]
    public async Task Demo_account_has_30_days_of_friend_history()
    {
        var token = await LoginAsync("demo@lineconsole.dev", "Demo1234");
        var accounts = (await GetDataAsync("/api/accounts", token)).EnumerateArray().ToArray();
        var d = await GetDataAsync($"/api/accounts/{accounts[0].GetProperty("id").GetString()}/dashboard", token);

        Assert.True(d.GetProperty("following").GetInt32() > 100);
        var daily = d.GetProperty("daily").EnumerateArray().ToArray();
        Assert.Equal(30, daily.Length);
        Assert.True(daily.Sum(x => x.GetProperty("follows").GetInt32()) > 50);
        Assert.Equal(d.GetProperty("following").GetInt32(), daily[^1].GetProperty("following").GetInt32());
    }

    [Fact]
    public async Task Demo_simulator_goes_through_the_real_webhook_pipeline()
    {
        var (token, accountId, _) = await NewAccountAsync();

        var (s1, follow) = await SendAsync(HttpMethod.Post, $"/api/accounts/{accountId}/demo/webhook-events", token, new { kind = "follow" });
        Assert.Equal(HttpStatusCode.OK, s1);
        Assert.Equal(1, follow.GetProperty("data").GetProperty("accepted").GetInt32());

        var (_, redelivery) = await SendAsync(HttpMethod.Post, $"/api/accounts/{accountId}/demo/webhook-events", token, new { kind = "redelivery" });
        Assert.Equal(1, redelivery.GetProperty("data").GetProperty("duplicates").GetInt32());
        Assert.Equal(follow.GetProperty("data").GetProperty("webhookEventId").GetString(), redelivery.GetProperty("data").GetProperty("webhookEventId").GetString());

        await ProcessInboxAsync();
        Assert.Equal(1, (await GetDataAsync($"/api/accounts/{accountId}/friends", token)).GetProperty("total").GetInt32());
    }

    // ---------- 工具 ----------

    private static void AssertDay(JsonElement day, DateOnly date, int follows, int unfollows, int following)
    {
        Assert.Equal(date.ToString("yyyy-MM-dd"), day.GetProperty("date").GetString());
        Assert.Equal(follows, day.GetProperty("follows").GetInt32());
        Assert.Equal(unfollows, day.GetProperty("unfollows").GetInt32());
        Assert.Equal(following, day.GetProperty("following").GetInt32());
    }

    private static string UserId() => "U" + Guid.NewGuid().ToString("N");

    /// <summary>LINE Messaging API 的事件格式</summary>
    private static JsonObject Event(string type, string? userId, DateTime occurredAtUtc) => new()
    {
        ["type"] = type,
        ["mode"] = "active",
        ["timestamp"] = new DateTimeOffset(occurredAtUtc, TimeSpan.Zero).ToUnixTimeMilliseconds(),
        ["source"] = new JsonObject { ["type"] = "user", ["userId"] = userId },
        ["webhookEventId"] = Guid.NewGuid().ToString("N")[..26].ToUpperInvariant(),
        ["deliveryContext"] = new JsonObject { ["isRedelivery"] = false }
    };

    private static string Payload(params JsonObject[] events) =>
        new JsonObject { ["destination"] = "U" + new string('0', 32), ["events"] = new JsonArray(events.Select(e => (JsonNode)e.DeepClone()).ToArray()) }.ToJsonString();

    private static string Sign(string secret, string body) =>
        Convert.ToBase64String(HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(body)));

    private async Task<(HttpStatusCode Status, JsonElement Body)> PostWebhookAsync(Guid accountId, string body, string? signature)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, $"/api/webhooks/line/{accountId}") { Content = new StringContent(body, Encoding.UTF8, "application/json") };
        if (signature is not null)
            req.Headers.Add("X-Line-Signature", signature);
        var res = await _client.SendAsync(req, Ct);
        return (res.StatusCode, JsonDocument.Parse(await res.Content.ReadAsStringAsync(Ct)).RootElement.Clone());
    }

    private async Task SendAndProcessAsync(Guid accountId, string secret, params JsonObject[] events)
    {
        var body = Payload(events);
        Assert.Equal(HttpStatusCode.OK, (await PostWebhookAsync(accountId, body, Sign(secret, body))).Status);
        await ProcessInboxAsync();
    }

    private async Task ProcessInboxAsync()
    {
        using var scope = _app.Services.CreateScope();
        while (await scope.ServiceProvider.GetRequiredService<IWebhookEventProcessor>().ProcessPendingAsync(100, Ct) > 0)
        {
        }
    }

    private async Task<int> CountInboxAsync(Guid accountId)
    {
        using var scope = _app.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().LineWebhookEvents.CountAsync(x => x.AccountId == accountId, Ct);
    }

    private async Task<LineWebhookEventStatus> InboxStatusAsync(Guid accountId, string webhookEventId)
    {
        using var scope = _app.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().LineWebhookEvents
            .Where(x => x.AccountId == accountId && x.WebhookEventId == webhookEventId).Select(x => x.Status).SingleAsync(Ct);
    }

    private async Task<string> LoginAsync(string email, string password)
    {
        var login = await _client.PostAsJsonAsync("/api/auth/login", new { email, password }, Ct);
        return (await login.Content.ReadFromJsonAsync<JsonElement>(Ct)).GetProperty("data").GetProperty("token").GetString()!;
    }

    /// <summary>新使用者＋一個新的官方帳號（每個測試各用各的，互不影響）</summary>
    private async Task<(string Token, Guid AccountId, string Secret)> NewAccountAsync()
    {
        var email = $"owner-{Guid.NewGuid():N}@example.com";
        (await _client.PostAsJsonAsync("/api/auth/register", new { email, password = "Passw0rd", displayName = "店家" }, Ct)).EnsureSuccessStatusCode();
        var token = await LoginAsync(email, "Passw0rd");
        var secret = "secret-" + Guid.NewGuid().ToString("N");
        var (status, created) = await SendAsync(HttpMethod.Post, "/api/accounts", token,
            new { channelName = "測試帳號", channelId = Random.Shared.Next(1_000_000, 9_999_999).ToString(), channelSecret = secret, channelAccessToken = "token-" + Guid.NewGuid().ToString("N") });
        Assert.Equal(HttpStatusCode.OK, status);
        return (token, Guid.Parse(created.GetProperty("data").GetProperty("id").GetString()!), secret);
    }

    private async Task<JsonElement> GetDataAsync(string url, string token)
    {
        var (status, body) = await GetAsync(url, token);
        Assert.Equal(HttpStatusCode.OK, status);
        return body.GetProperty("data");
    }

    private Task<(HttpStatusCode Status, JsonElement Body)> GetAsync(string url, string token) => SendAsync(HttpMethod.Get, url, token);

    private async Task<(HttpStatusCode Status, JsonElement Body)> SendAsync(HttpMethod method, string url, string token, object? json = null)
    {
        var req = new HttpRequestMessage(method, url) { Content = json is null ? null : JsonContent.Create(json) };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var res = await _client.SendAsync(req, Ct);
        var text = await res.Content.ReadAsStringAsync(Ct);
        return (res.StatusCode, text.Length == 0 ? default : JsonDocument.Parse(text).RootElement.Clone());
    }
}
