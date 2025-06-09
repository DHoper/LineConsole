namespace LineConsole.Application.LineOfficialAccounts.Interfaces;

/// <summary>
/// 提供 LINE 官方帳號的存取功能，例如查詢 Access Token
/// </summary>
public interface ILineOfficialAccountRepository
{
    /// <summary>
    /// 根據 LINE 官方帳號 ID 取得對應的 Channel Access Token
    /// </summary>
    /// <param name="lineOfficialAccountId">LINE 官方帳號 ID</param>
    /// <param name="ct">取消權杖</param>
    Task<string> GetAccessTokenAsync(Guid lineOfficialAccountId, CancellationToken ct = default);
}
