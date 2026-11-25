namespace LineConsole.Application.LineOfficialAccounts.Models;

/// <summary>官方帳號清單的一筆。不含 Secret、Access Token：這兩個只進不出</summary>
public sealed record OfficialAccountSummary(Guid Id, string ChannelName, string ChannelId, string? BasicId, DateTime CreatedAt);

/// <summary>新增官方帳號：在 LINE Developers 後台的 Messaging API 頁面可以找到這些值</summary>
public sealed record CreateOfficialAccountInput
{
    /// <summary>顯示名稱；不填就用 LINE 上的名稱</summary>
    public string? ChannelName { get; init; }
    public string ChannelId { get; init; } = string.Empty;
    public string ChannelSecret { get; init; } = string.Empty;
    public string ChannelAccessToken { get; init; } = string.Empty;
}

/// <summary>LINE 的 GET /v2/bot/info 回應（只取用得到的欄位）</summary>
public sealed record LineBotInfo
{
    public string UserId { get; init; } = string.Empty;
    public string BasicId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? PictureUrl { get; init; }
}
