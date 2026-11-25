using System.Collections.Concurrent;
using System.Text;
using System.Text.Json.Nodes;
using LineConsole.Application.Common.Models;
using LineConsole.Application.Friends;
using LineConsole.Application.LineOfficialAccounts.Interfaces;
using LineConsole.Application.Webhooks;
using LineConsole.Domain.Entities;

namespace LineConsole.Infrastructure.Demo;

/// <summary>展示模式按一下「模擬事件」的結果</summary>
public sealed record DemoWebhookResult(string Kind, int Accepted, int Duplicates, string WebhookEventId, string? LineUserId);

/// <summary>每個帳號最後送出的事件（「模擬重送」要原封不動再送一次）</summary>
public sealed class DemoWebhookLog
{
    private readonly ConcurrentDictionary<Guid, string> _last = new();
    public void Remember(Guid accountId, JsonObject e) => _last[accountId] = e.ToJsonString();
    public JsonObject? Last(Guid accountId) => _last.TryGetValue(accountId, out var json) ? JsonNode.Parse(json) as JsonObject : null;
}

/// <summary>
/// 展示模式沒有真的 LINE 會打 Webhook 進來，所以由這裡「扮演 LINE」：
/// 組出跟 LINE 一樣格式的事件、用該帳號的 Channel Secret 算 X-Line-Signature，再交給正式的 <see cref="ILineWebhookService"/>。
/// 驗簽、去重、收件匣、背景處理走的都是正式流程，只有「誰送來的」是模擬的。
/// </summary>
public sealed class DemoWebhookSimulator
{
    public static readonly string[] Kinds = ["follow", "unfollow", "message", "redelivery"];

    private static readonly string[] Messages = ["請問今天有營業嗎？", "我想訂位", "集點卡怎麼用？", "優惠券在哪裡看", "謝謝！", "可以外送嗎？"];

    private readonly ILineOfficialAccountRepository _accounts;
    private readonly ILineWebhookService _webhook;
    private readonly IFriendRepository _friends;
    private readonly IOfficialAccountAccess _access;
    private readonly TimeProvider _time;
    private readonly DemoWebhookLog _log;

    public DemoWebhookSimulator(ILineOfficialAccountRepository accounts, ILineWebhookService webhook, IFriendRepository friends,
        IOfficialAccountAccess access, TimeProvider time, DemoWebhookLog log)
    {
        _accounts = accounts;
        _webhook = webhook;
        _friends = friends;
        _access = access;
        _time = time;
        _log = log;
    }

    /// <summary>畫面上的「模擬事件」按鈕：新好友加入、好友封鎖、好友傳訊息、LINE 重送上一個事件</summary>
    public async Task<DemoWebhookResult> SimulateAsync(Guid accountId, string kind, CancellationToken ct = default)
    {
        await _access.EnsureOwnerAsync(accountId, ct);
        var now = _time.GetUtcNow().UtcDateTime;
        var rng = Random.Shared;

        JsonObject e;
        switch (kind)
        {
            case "follow":
                e = Event("follow", NewUserId(rng), now, rng);
                break;
            case "unfollow":
                e = Event("unfollow", await PickFollowingAsync(accountId, rng, ct), now, rng);
                break;
            case "message":
                e = Event("message", await PickFollowingAsync(accountId, rng, ct), now, rng, text: Messages[rng.Next(Messages.Length)]);
                break;
            case "redelivery":
                e = _log.Last(accountId) ?? throw new AppException("DEMO_NOTHING_TO_REDELIVER", "請先模擬一個事件，才有東西可以重送");
                e["deliveryContext"] = new JsonObject { ["isRedelivery"] = true };   // 同一個 webhookEventId，只是標成重送
                break;
            default:
                throw new AppException("DEMO_UNKNOWN_KIND", $"不支援的模擬事件：{kind}");
        }

        var result = await SendAsync(accountId, [e], ct);
        return new DemoWebhookResult(kind, result.Accepted, result.Duplicates, (string)e["webhookEventId"]!, (string?)e["source"]?["userId"]);
    }

    /// <summary>組成 LINE 的 Webhook 請求、簽章，交給正式的處理流程</summary>
    public async Task<WebhookIngestResult> SendAsync(Guid accountId, IReadOnlyList<JsonObject> events, CancellationToken ct = default)
    {
        var secret = await _accounts.GetChannelSecretAsync(accountId, ct)
                     ?? throw new NotFoundException("ACCOUNT_NOT_FOUND", "找不到這個 LINE 官方帳號");
        var body = Encoding.UTF8.GetBytes(new JsonObject
        {
            ["destination"] = "U" + accountId.ToString("N"),
            ["events"] = new JsonArray(events.Select(x => (JsonNode)x.DeepClone()).ToArray())
        }.ToJsonString());

        var result = await _webhook.IngestAsync(accountId, body, LineSignature.Compute(secret, body), ct);
        if (events.Count > 0)
            _log.Remember(accountId, events[^1]);
        return result;
    }

    /// <summary>
    /// 展示資料：產生過去 30 天的好友事件（加入、封鎖、重新加入、傳訊息），依時間先後分批送進 Webhook。
    /// 亂數種子固定，每次啟動的數字都一樣。
    /// </summary>
    public async Task SeedHistoryAsync(Guid accountId, DemoData.Audience audience, TimeZoneInfo zone, CancellationToken ct = default)
    {
        var rng = new Random(audience.Seed);
        var now = _time.GetUtcNow().UtcDateTime;
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(now, zone));
        var firstDay = today.AddDays(-(FriendService.Days - 1));
        var events = new List<JsonObject>();
        var following = new List<string>();
        var blocked = new List<string>();

        // 視窗開始前就加入的老朋友
        for (var i = 0; i < audience.ExistingFriends; i++)
        {
            var user = NewUserId(rng);
            events.Add(Event("follow", user, At(firstDay.AddDays(-rng.Next(1, 90)), zone, now, rng), rng));
            following.Add(user);
        }

        for (var day = firstDay; day <= today; day = day.AddDays(1))
        {
            var daysAgo = today.DayNumber - day.DayNumber;
            var weekend = day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
            var follows = Math.Max(0, (weekend ? audience.WeekendFollows : audience.WeekdayFollows) + rng.Next(-1, 3));
            if (daysAgo == audience.CampaignDaysAgo) follows += 25;        // 辦活動當天加入暴增
            if (daysAgo == audience.CampaignDaysAgo - 1) follows += 8;
            var churn = audience.DailyChurn * (daysAgo is var d && d < audience.CampaignDaysAgo && d >= audience.CampaignDaysAgo - 3 ? 3 : 1);  // 活動後幾天封鎖也比較多
            var unfollows = (int)Math.Round(following.Count * churn + rng.NextDouble() - 0.5);

            var before = following.ToList();   // 只從「今天以前就是好友」的人挑封鎖、訊息，時間順序才合理
            for (var i = 0; i < unfollows && before.Count > 0; i++)
            {
                var user = before[rng.Next(before.Count)];
                before.Remove(user);
                following.Remove(user);
                blocked.Add(user);
                events.Add(Event("unfollow", user, At(day, zone, now, rng), rng));
            }
            if (blocked.Count > 0 && rng.NextDouble() < 0.25)   // 偶爾有人解除封鎖
            {
                var user = blocked[rng.Next(blocked.Count)];
                if (!events.Any(x => (string?)x["source"]?["userId"] == user && DayOf(x, zone) == day))
                {
                    blocked.Remove(user);
                    following.Add(user);
                    events.Add(Event("follow", user, At(day, zone, now, rng), rng, unblocked: true));
                }
            }
            for (var i = 0; i < follows; i++)
            {
                var user = NewUserId(rng);
                following.Add(user);
                events.Add(Event("follow", user, At(day, zone, now, rng), rng));
            }
            for (var i = rng.Next(2, 7); i > 0 && before.Count > 0; i--)
                events.Add(Event("message", before[rng.Next(before.Count)], At(day, zone, now, rng), rng, text: Messages[rng.Next(Messages.Length)]));
        }

        foreach (var chunk in events.OrderBy(x => (long)x["timestamp"]!).Chunk(100))
            await SendAsync(accountId, chunk, ct);
    }

    private async Task<string> PickFollowingAsync(Guid accountId, Random rng, CancellationToken ct)
    {
        var (count, _) = await _friends.CountAsync(accountId, ct);
        if (count == 0)
            throw new AppException("DEMO_NO_FRIENDS", "目前沒有好友，請先模擬「新好友加入」");
        var (items, _) = await _friends.SearchAsync(accountId, LineFriendStatus.Following, null, rng.Next(count), 1, ct);
        return items[0].LineUserId;
    }

    /// <summary>跟 LINE Messaging API 一樣格式的事件</summary>
    public static JsonObject Event(string type, string userId, DateTime occurredAtUtc, Random rng, string? text = null, bool unblocked = false)
    {
        var e = new JsonObject
        {
            ["type"] = type,
            ["mode"] = "active",
            ["timestamp"] = new DateTimeOffset(occurredAtUtc, TimeSpan.Zero).ToUnixTimeMilliseconds(),
            ["source"] = new JsonObject { ["type"] = "user", ["userId"] = userId },
            ["webhookEventId"] = NewEventId(occurredAtUtc, rng),
            ["deliveryContext"] = new JsonObject { ["isRedelivery"] = false }
        };
        if (type == "follow")
        {
            e["replyToken"] = Hex(rng, 32);
            e["follow"] = new JsonObject { ["isUnblocked"] = unblocked };
        }
        else if (type == "message")
        {
            e["replyToken"] = Hex(rng, 32);
            e["message"] = new JsonObject { ["id"] = rng.NextInt64(10_000_000_000_000, 99_999_999_999_999).ToString(), ["type"] = "text", ["text"] = text ?? "" };
        }
        return e;
    }

    public static string NewUserId(Random rng) => "U" + Hex(rng, 32);

    /// <summary>LINE 的 webhookEventId 是 ULID：前 10 碼是時間、後 16 碼亂數（Crockford Base32）</summary>
    public static string NewEventId(DateTime atUtc, Random rng)
    {
        const string alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
        Span<char> c = stackalloc char[26];
        var ms = new DateTimeOffset(atUtc, TimeSpan.Zero).ToUnixTimeMilliseconds();
        for (var i = 9; i >= 0; i--, ms >>= 5)
            c[i] = alphabet[(int)(ms & 31)];
        for (var i = 10; i < 26; i++)
            c[i] = alphabet[rng.Next(32)];
        return new string(c);
    }

    private static string Hex(Random rng, int length)
    {
        Span<byte> bytes = stackalloc byte[length / 2];
        rng.NextBytes(bytes);
        return Convert.ToHexStringLower(bytes);
    }

    /// <summary>那一天（當地時間）08:00～23:00 之間的隨機時間；今天的話不超過現在</summary>
    private static DateTime At(DateOnly day, TimeZoneInfo zone, DateTime nowUtc, Random rng)
    {
        var start = TimeZoneInfo.ConvertTimeToUtc(day.ToDateTime(new TimeOnly(8, 0)), zone);
        var end = TimeZoneInfo.ConvertTimeToUtc(day.ToDateTime(new TimeOnly(23, 0)), zone);
        if (end > nowUtc.AddMinutes(-1)) end = nowUtc.AddMinutes(-1);
        if (end <= start) start = TimeZoneInfo.ConvertTimeToUtc(day.ToDateTime(TimeOnly.MinValue), zone);
        if (end <= start) return start;
        return start.AddSeconds(rng.NextDouble() * (end - start).TotalSeconds);
    }

    private static DateOnly DayOf(JsonObject e, TimeZoneInfo zone) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeMilliseconds((long)e["timestamp"]!).UtcDateTime, zone));
}
