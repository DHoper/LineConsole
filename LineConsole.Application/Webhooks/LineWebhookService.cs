using System.Text.Json;
using LineConsole.Application.Common.Models;
using LineConsole.Application.LineOfficialAccounts.Interfaces;
using LineConsole.Domain.Entities;

namespace LineConsole.Application.Webhooks;

public interface ILineWebhookService
{
    /// <summary>
    /// 收 Webhook：驗證簽章 → 每個事件存進收件匣（重複的跳過）→ 通知背景服務。
    /// 不在這裡處理事件：LINE 要求盡快回 200，處理交給背景服務（先回應、後處理）。
    /// </summary>
    Task<WebhookIngestResult> IngestAsync(Guid lineOfficialAccountId, byte[] rawBody, string? signature, CancellationToken ct = default);
}

public class LineWebhookService : ILineWebhookService
{
    private readonly ILineOfficialAccountRepository _accounts;
    private readonly IWebhookInboxRepository _inbox;
    private readonly IWebhookInboxNotifier _notifier;
    private readonly TimeProvider _time;

    public LineWebhookService(ILineOfficialAccountRepository accounts, IWebhookInboxRepository inbox, IWebhookInboxNotifier notifier, TimeProvider time)
    {
        _accounts = accounts;
        _inbox = inbox;
        _notifier = notifier;
        _time = time;
    }

    public async Task<WebhookIngestResult> IngestAsync(Guid lineOfficialAccountId, byte[] rawBody, string? signature, CancellationToken ct = default)
    {
        var secret = await _accounts.GetChannelSecretAsync(lineOfficialAccountId, ct)
                     ?? throw new NotFoundException("ACCOUNT_NOT_FOUND", "找不到這個 LINE 官方帳號");

        if (!LineSignature.IsValid(secret, rawBody, signature))
            throw new WebhookSignatureException();

        using var doc = ParseOrThrow(rawBody);
        if (!doc.RootElement.TryGetProperty("events", out var events) || events.ValueKind != JsonValueKind.Array)
            throw new AppException("WEBHOOK_INVALID", "缺少 events");

        // LINE 設定 Webhook URL 時按「Verify」會送一個空的 events：簽章對就回 200
        var now = _time.GetUtcNow().UtcDateTime;
        int accepted = 0, duplicates = 0;
        foreach (var e in events.EnumerateArray())
        {
            var webhookEventId = Str(e, "webhookEventId");
            if (string.IsNullOrEmpty(webhookEventId))
                continue;   // 沒有事件 ID 的（非常舊的格式）無法去重，不收

            var occurredAt = e.TryGetProperty("timestamp", out var ts) && ts.TryGetInt64(out var ms)
                ? DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime
                : now;
            var isRedelivery = e.TryGetProperty("deliveryContext", out var dc) && dc.TryGetProperty("isRedelivery", out var r) && r.ValueKind == JsonValueKind.True;
            var userId = e.TryGetProperty("source", out var src) ? Str(src, "userId") : null;

            var row = LineWebhookEvent.Receive(lineOfficialAccountId, webhookEventId, Str(e, "type") ?? "unknown", userId,
                occurredAt, isRedelivery, e.GetRawText(), now);

            if (await _inbox.TryAddAsync(row, ct)) accepted++;
            else duplicates++;
        }

        if (accepted > 0)
            _notifier.Notify();
        return new WebhookIngestResult(accepted, duplicates);
    }

    private static JsonDocument ParseOrThrow(byte[] body)
    {
        try
        {
            return JsonDocument.Parse(body);
        }
        catch (JsonException)
        {
            throw new AppException("WEBHOOK_INVALID", "請求內容不是合法的 JSON");
        }
    }

    private static string? Str(JsonElement e, string name) =>
        e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
}
