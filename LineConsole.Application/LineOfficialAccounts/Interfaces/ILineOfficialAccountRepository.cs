using LineConsole.Domain.Entities;

namespace LineConsole.Application.LineOfficialAccounts.Interfaces;

/// <summary>
/// 提供 LINE 官方帳號的存取功能，例如新增與查詢 Access Token
/// </summary>
public interface ILineOfficialAccountRepository
{
    /// <summary>
    /// 根據 LINE 官方帳號 ID 取得對應的 Channel Access Token。
    /// 不檢查擁有者，只給背景工作（排程）用；使用者的請求請走 <see cref="IOfficialAccountAccess"/>。
    /// </summary>
    Task<string> GetAccessTokenAsync(Guid lineOfficialAccountId, CancellationToken ct = default);

    /// <summary>
    /// 取得屬於指定 Identity 使用者的官方帳號的 Access Token；不存在或不屬於他時回傳 null
    /// </summary>
    Task<string?> FindAccessTokenForOwnerAsync(Guid lineOfficialAccountId, string identityUserId, CancellationToken ct = default);

    /// <summary>
    /// 新增一筆 LINE 官方帳號綁定資料
    /// </summary>
    Task AddAsync(LineOfficialAccount account, CancellationToken ct = default);

    /// <summary>
    /// 透過 UserProfile 取得 LineOfficialAccount 清單
    /// </summary>
    Task<List<LineOfficialAccount>> FindByUserProfileIdAsync(Guid userProfileId, CancellationToken ct = default);

}
