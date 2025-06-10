namespace LineConsole.Application.LineOfficialAccounts.Models;

/// <summary>
/// 用於建立 LINE 官方帳號的應用層輸入模型（通常用於 Service 或 UseCase）
/// </summary>
public class CreateLineOfficialAccountInput
{
    public Guid UserProfileId { get; init; } // 綁定的使用者 ID

    public string ChannelId { get; init; } = string.Empty; // LINE Channel ID

    public string ChannelSecret { get; init; } = string.Empty; // Channel Secret，用於 webhook 驗證

    public string ChannelAccessToken { get; init; } = string.Empty; // Access Token，用於呼叫 LINE API

    public string? ChannelName { get; init; } // 顯示用名稱（可選）
}
