namespace LineConsole.Application.LineOfficialAccounts.Models;

/// <summary>
/// 前端使用的 LINE 官方帳號簡要資訊
/// </summary>
public record class LineOfficialAccountViewModel
{
    public Guid Id { get; init; }

    public string ChannelId { get; init; } = string.Empty;

    public string ChannelName { get; init; } = string.Empty;
}
