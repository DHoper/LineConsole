namespace LineConsole.Application.LineOfficialAccounts.Models;

/// <summary>
/// �e�ݨϥΪ� LINE �x��b��²�n��T
/// </summary>
public record class LineOfficialAccountViewModel
{
    public Guid Id { get; init; }

    public string ChannelId { get; init; } = string.Empty;

    public string ChannelName { get; init; } = string.Empty;
}
