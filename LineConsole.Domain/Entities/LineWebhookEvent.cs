namespace LineConsole.Domain.Entities;

public enum LineWebhookEventStatus
{
    /// <summary>收到、還沒處理</summary>
    Pending = 0,
    Processed = 1,
    /// <summary>不需要處理的事件類型（例如 join、postback），留著當紀錄</summary>
    Ignored = 2,
    Failed = 3
}

/// <summary>
/// Webhook 收件匣（Inbox）的一筆事件。
/// 收到 Webhook 時先驗證簽章、把每個事件存進來，馬上回 200 給 LINE；背景服務再依發生時間處理。
/// 同一個官方帳號的 webhookEventId 不能重複（資料庫唯一索引）：LINE 重送的事件只會存一次，也只會處理一次。
/// </summary>
public class LineWebhookEvent
{
    public Guid Id { get; init; }
    public Guid LineOfficialAccountId { get; init; }

    /// <summary>LINE 給的事件 ID（ULID），用來去重</summary>
    public string WebhookEventId { get; init; } = string.Empty;

    /// <summary>follow、unfollow、message…</summary>
    public string EventType { get; init; } = string.Empty;

    public string? LineUserId { get; init; }

    /// <summary>事件發生的時間（LINE 的 timestamp，UTC）</summary>
    public DateTime OccurredAt { get; init; }

    /// <summary>LINE 標示的重送</summary>
    public bool IsRedelivery { get; init; }

    /// <summary>原始的事件 JSON（除錯、重新處理用）</summary>
    public string Payload { get; init; } = string.Empty;

    public LineWebhookEventStatus Status { get; private set; }
    public DateTime ReceivedAt { get; init; }
    public DateTime? ProcessedAt { get; private set; }
    public string? Error { get; private set; }

    public static LineWebhookEvent Receive(Guid accountId, string webhookEventId, string eventType, string? lineUserId,
        DateTime occurredAt, bool isRedelivery, string payload, DateTime nowUtc) => new()
    {
        Id = Guid.NewGuid(),
        LineOfficialAccountId = accountId,
        WebhookEventId = webhookEventId,
        EventType = eventType,
        LineUserId = lineUserId,
        OccurredAt = occurredAt,
        IsRedelivery = isRedelivery,
        Payload = payload,
        Status = LineWebhookEventStatus.Pending,
        ReceivedAt = nowUtc
    };

    public static LineWebhookEvent Load(Guid id, Guid accountId, string webhookEventId, string eventType, string? lineUserId, DateTime occurredAt,
        bool isRedelivery, string payload, LineWebhookEventStatus status, DateTime receivedAt, DateTime? processedAt, string? error)
    {
        var e = new LineWebhookEvent
        {
            Id = id, LineOfficialAccountId = accountId, WebhookEventId = webhookEventId, EventType = eventType, LineUserId = lineUserId,
            OccurredAt = occurredAt, IsRedelivery = isRedelivery, Payload = payload, ReceivedAt = receivedAt
        };
        e.Status = status;
        e.ProcessedAt = processedAt;
        e.Error = error;
        return e;
    }

    public void MarkProcessed(DateTime nowUtc) { Status = LineWebhookEventStatus.Processed; ProcessedAt = nowUtc; }

    /// <summary>不需要處理（不支援的事件類型、或狀態沒有改變），reason 記在 Error 欄方便查</summary>
    public void MarkIgnored(DateTime nowUtc, string? reason = null) { Status = LineWebhookEventStatus.Ignored; ProcessedAt = nowUtc; Error = reason; }

    public void MarkFailed(string error, DateTime nowUtc)
    {
        Status = LineWebhookEventStatus.Failed;
        ProcessedAt = nowUtc;
        Error = error.Length > 500 ? error[..500] : error;
    }
}
