namespace LineConsole.Application.LineOfficialAccounts.Models;

/// <summary>
/// �Ω�إ� LINE �x��b�������μh��J�ҫ��]�q�`�Ω� Service �� UseCase�^
/// </summary>
public class CreateLineOfficialAccountInput
{
    public Guid UserProfileId { get; init; } // �j�w���ϥΪ� ID

    public string ChannelId { get; init; } = string.Empty; // LINE Channel ID

    public string ChannelSecret { get; init; } = string.Empty; // Channel Secret�A�Ω� webhook ����

    public string ChannelAccessToken { get; init; } = string.Empty; // Access Token�A�Ω�I�s LINE API

    public string? ChannelName { get; init; } // ��ܥΦW�١]�i��^
}
