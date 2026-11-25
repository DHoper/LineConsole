namespace LineConsole.Domain.Entities;

public enum LineFriendStatus
{
    /// <summary>追蹤中（加了好友）</summary>
    Following = 0,
    /// <summary>封鎖了官方帳號（LINE 的 unfollow 事件）</summary>
    Blocked = 1
}

/// <summary>
/// 官方帳號的好友。狀態只由 Webhook 事件改變：follow、unfollow、message。
/// LINE 不保證事件送達的順序（還會重送），所以每個事件都帶發生時間，比目前狀態舊的事件直接忽略。
/// </summary>
public class LineFriend
{
    public Guid Id { get; init; }
    public Guid LineOfficialAccountId { get; init; }
    public string LineUserId { get; init; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? PictureUrl { get; set; }
    public LineFriendStatus Status { get; private set; }
    public DateTime FollowedAt { get; private set; }
    public DateTime? BlockedAt { get; private set; }
    public DateTime? LastMessageAt { get; private set; }

    /// <summary>最後一次改變追蹤狀態的事件時間：比這個舊的 follow／unfollow 不套用</summary>
    public DateTime LastStatusEventAt { get; private set; }

    public DateTime CreatedAt { get; init; }

    /// <summary>第一次看到這個使用者（通常是 follow 事件）</summary>
    public static LineFriend FirstSeen(Guid accountId, string lineUserId, DateTime occurredAt, DateTime nowUtc) => new()
    {
        Id = Guid.NewGuid(),
        LineOfficialAccountId = accountId,
        LineUserId = lineUserId,
        Status = LineFriendStatus.Following,
        FollowedAt = occurredAt,
        LastStatusEventAt = DateTime.MinValue,
        CreatedAt = nowUtc
    };

    public static LineFriend Load(Guid id, Guid accountId, string lineUserId, string? displayName, string? pictureUrl, LineFriendStatus status,
        DateTime followedAt, DateTime? blockedAt, DateTime? lastMessageAt, DateTime lastStatusEventAt, DateTime createdAt)
    {
        var f = new LineFriend
        {
            Id = id, LineOfficialAccountId = accountId, LineUserId = lineUserId, DisplayName = displayName, PictureUrl = pictureUrl, CreatedAt = createdAt
        };
        f.Status = status;
        f.FollowedAt = followedAt;
        f.BlockedAt = blockedAt;
        f.LastMessageAt = lastMessageAt;
        f.LastStatusEventAt = lastStatusEventAt;
        return f;
    }

    /// <summary>加好友（或解除封鎖）。回傳有沒有真的改變狀態</summary>
    public bool Follow(DateTime occurredAt)
    {
        if (occurredAt < LastStatusEventAt) return false;   // 比較舊的事件晚到：忽略
        var changed = Status != LineFriendStatus.Following || LastStatusEventAt == DateTime.MinValue;
        if (Status == LineFriendStatus.Blocked || LastStatusEventAt == DateTime.MinValue)
            FollowedAt = occurredAt;
        Status = LineFriendStatus.Following;
        BlockedAt = null;
        LastStatusEventAt = occurredAt;
        return changed;
    }

    /// <summary>封鎖。回傳有沒有真的改變狀態</summary>
    public bool Block(DateTime occurredAt)
    {
        if (occurredAt < LastStatusEventAt) return false;
        var changed = Status != LineFriendStatus.Blocked;
        Status = LineFriendStatus.Blocked;
        BlockedAt = occurredAt;
        LastStatusEventAt = occurredAt;
        return changed;
    }

    /// <summary>傳訊息給官方帳號：只更新最後互動時間</summary>
    public void Message(DateTime occurredAt)
    {
        if (LastMessageAt is null || occurredAt > LastMessageAt)
            LastMessageAt = occurredAt;
    }
}
