namespace LineConsole.Application.LineOfficialAccounts.Interfaces;

/// <summary>
/// ���� LINE �x��b�����s���\��A�Ҧp�d�� Access Token
/// </summary>
public interface ILineOfficialAccountRepository
{
    /// <summary>
    /// �ھ� LINE �x��b�� ID ���o������ Channel Access Token
    /// </summary>
    /// <param name="lineOfficialAccountId">LINE �x��b�� ID</param>
    /// <param name="ct">�����v��</param>
    Task<string> GetAccessTokenAsync(Guid lineOfficialAccountId, CancellationToken ct = default);
}
