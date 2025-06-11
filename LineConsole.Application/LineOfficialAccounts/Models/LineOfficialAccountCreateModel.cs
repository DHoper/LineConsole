namespace LineConsole.Application.LineOfficialAccounts.Models;

/// <summary>
/// 建立 LINE 官方帳號的資料模型
/// </summary>
public class LineOfficialAccountCreateModel
{
    public string ChannelId { get; init; } = string.Empty;              // LINE Channel ID（唯一識別）

    public string ChannelSecret { get; init; } = string.Empty;          // 用於 webhook 驗證的 channel secret

    public string ChannelAccessToken { get; init; } = string.Empty;     // channel access token

    public string ChannelName { get; init; } = string.Empty;            // LINE 官方帳號名稱
}
