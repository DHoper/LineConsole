using LineConsole.Domain.Entities;

namespace LineConsole.Application.LineOfficialAccounts.Interfaces;

/// <summary>
/// 提供 LINE 官方帳號的存取功能，例如新增與查詢 Access Token
/// </summary>
public interface ILineOfficialAccountRepository
{
    /// <summary>
    /// 根據 LINE 官方帳號 ID 取得對應的 Channel Access Token
    /// </summary>
    Task<string> GetAccessTokenAsync(Guid lineOfficialAccountId, CancellationToken ct = default);

    /// <summary>
    /// 新增一筆 LINE 官方帳號綁定資料
    /// </summary>
    Task AddAsync(LineOfficialAccount account, CancellationToken ct = default);
}
