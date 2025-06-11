using System;

namespace LineConsole.Domain.Entities;

/// <summary>
/// LINE 官方帳號實體，綁定後台使用者與其 LINE channel 資訊
/// </summary>
public class LineOfficialAccount
{
    /// <summary>官方帳號主鍵 ID</summary>
    public Guid Id { get; init; }

    /// <summary>所屬後台使用者個人資料 ID</summary>
    public Guid UserProfileId { get; init; }

    /// <summary>所綁定的後台使用者</summary>
    public virtual UserProfile UserProfile { get; set; } = default!;

    /// <summary>LINE Channel ID（即原 LineUserId，系統中唯一）</summary>
    public string ChannelId { get; set; } = string.Empty;

    /// <summary>LINE Channel 名稱（顯示用）</summary>
    public string ChannelName { get; set; } = string.Empty;

    /// <summary>LINE Channel Access Token（建議加密儲存）</summary>
    public string ChannelAccessToken { get; set; } = string.Empty;

    /// <summary>LINE Channel Secret，用於驗證 webhook</summary>
    public string ChannelSecret { get; set; } = string.Empty;

    /// <summary>建立時間（UTC）</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>最後更新時間（UTC）</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>建立一個新的官方帳號實體</summary>
    public static LineOfficialAccount Create(
        Guid userProfileId,
        string channelId,
        string channelSecret,
        string channelAccessToken,
        string channelName)
    {
        var now = DateTime.UtcNow;
        return new LineOfficialAccount
        {
            Id = Guid.NewGuid(),
            UserProfileId = userProfileId,
            ChannelId = channelId,
            ChannelSecret = channelSecret,
            ChannelAccessToken = channelAccessToken,
            ChannelName = channelName,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public static LineOfficialAccount Load(
     Guid id,
     Guid userProfileId,
     string channelId,
     string channelSecret,
     string channelAccessToken,
     string channelName,
     DateTime createdAt,
     DateTime updatedAt)
    {
        return new LineOfficialAccount
        {
            Id = id,
            UserProfileId = userProfileId,
            ChannelId = channelId,
            ChannelSecret = channelSecret,
            ChannelAccessToken = channelAccessToken,
            ChannelName = channelName,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }
}
