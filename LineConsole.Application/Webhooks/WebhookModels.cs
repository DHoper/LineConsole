using LineConsole.Domain.Entities;

namespace LineConsole.Application.Webhooks;

/// <summary>一次 Webhook 請求的結果：存進收件匣幾筆、重複（重送）跳過幾筆</summary>
public sealed record WebhookIngestResult(int Accepted, int Duplicates);

/// <summary>簽章不對（或缺少簽章）：不是 LINE 送來的，拒絕</summary>
public sealed class WebhookSignatureException() : Exception("X-Line-Signature 驗證失敗");

public interface IWebhookInboxRepository
{
    /// <summary>存進收件匣；同一個帳號的 webhookEventId 已經存過（LINE 重送）就回傳 false</summary>
    Task<bool> TryAddAsync(LineWebhookEvent webhookEvent, CancellationToken ct = default);

    /// <summary>待處理的事件，依發生時間先後</summary>
    Task<List<LineWebhookEvent>> ListPendingAsync(int max, CancellationToken ct = default);

    Task UpdateAsync(LineWebhookEvent webhookEvent, CancellationToken ct = default);

    /// <summary>某段時間以來處理過的 follow、unfollow 事件（儀表板的每日統計）</summary>
    Task<List<(string EventType, DateTime OccurredAt)>> ListProcessedStatusEventsAsync(Guid lineOfficialAccountId, DateTime sinceUtc, CancellationToken ct = default);
}

/// <summary>通知背景服務「收件匣有新事件」，不用等下一輪輪詢</summary>
public interface IWebhookInboxNotifier
{
    void Notify();
}

public interface IFriendRepository
{
    Task<LineFriend?> FindAsync(Guid lineOfficialAccountId, string lineUserId, CancellationToken ct = default);
    Task AddAsync(LineFriend friend, CancellationToken ct = default);
    Task UpdateAsync(LineFriend friend, CancellationToken ct = default);

    /// <summary>分頁查詢：狀態、名稱關鍵字；依加入時間新的在前</summary>
    Task<(List<LineFriend> Items, int Total)> SearchAsync(Guid lineOfficialAccountId, LineFriendStatus? status, string? keyword, int skip, int take, CancellationToken ct = default);

    Task<(int Following, int Blocked)> CountAsync(Guid lineOfficialAccountId, CancellationToken ct = default);
}

/// <summary>LINE 的 GET /v2/bot/profile/{userId} 回應</summary>
public sealed record LineUserProfile
{
    public string UserId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? PictureUrl { get; init; }
}
