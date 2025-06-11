namespace LineConsole.Application.LineOfficialAccounts.Models;

/// <summary>
/// �إ� LINE �x��b������Ƽҫ�
/// </summary>
public class LineOfficialAccountCreateModel
{
    public string ChannelId { get; init; } = string.Empty;              // LINE Channel ID�]�ߤ@�ѧO�^

    public string ChannelSecret { get; init; } = string.Empty;          // �Ω� webhook ���Ҫ� channel secret

    public string ChannelAccessToken { get; init; } = string.Empty;     // channel access token

    public string ChannelName { get; init; } = string.Empty;            // LINE �x��b���W��
}
