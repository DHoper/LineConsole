using LineConsole.Domain.Entities;

namespace LineConsole.Application.LineOfficialAccounts.Interfaces;

/// <summary>
/// ���� LINE �x��b�����s���\��A�Ҧp�s�W�P�d�� Access Token
/// </summary>
public interface ILineOfficialAccountRepository
{
    /// <summary>
    /// �ھ� LINE �x��b�� ID ���o������ Channel Access Token
    /// </summary>
    Task<string> GetAccessTokenAsync(Guid lineOfficialAccountId, CancellationToken ct = default);

    /// <summary>
    /// �s�W�@�� LINE �x��b���j�w���
    /// </summary>
    Task AddAsync(LineOfficialAccount account, CancellationToken ct = default);
}
