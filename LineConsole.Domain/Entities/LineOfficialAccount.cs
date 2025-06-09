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
    public UserProfile UserProfile { get; init; } = default!;

    /// <summary>LINE 平台上的 userId（channel ID），系統中唯一</summary>
    public string LineUserId { get; init; } = string.Empty;

    /// <summary>LINE 官方帳號名稱（顯示用）</summary>
    public string? ChannelName { get; init; }

    /// <summary>LINE channel access token（建議加密儲存）</summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>建立時間（UTC）</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>最後更新時間（UTC）</summary>
    public DateTime UpdatedAt { get; init; }

    /// <summary>
    /// 建立一個新的官方帳號實體
    /// </summary>
    public static LineOfficialAccount Create(
        Guid userProfileId,
        string lineUserId,
        string accessToken,
        string? channelName = null)
    {
        var now = DateTime.UtcNow;
        return new LineOfficialAccount
        {
            Id = Guid.NewGuid(),
            UserProfileId = userProfileId,
            LineUserId = lineUserId,
            AccessToken = accessToken,
            ChannelName = channelName,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
