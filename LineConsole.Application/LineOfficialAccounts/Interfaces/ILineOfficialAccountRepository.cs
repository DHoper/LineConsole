using LineConsole.Domain.Entities;

namespace LineConsole.Application.LineOfficialAccounts.Interfaces;

/// <summary>
/// LINE 官方帳號的資料存取。Channel Secret、Access Token 在資料庫裡是加密的，進出這裡時自動加解密。
/// </summary>
public interface ILineOfficialAccountRepository
{
    /// <summary>
    /// 根據官方帳號 ID 取得 Channel Access Token。
    /// 不檢查擁有者，只給背景工作（排程）用；使用者的請求請走 <see cref="IOfficialAccountAccess"/>。
    /// </summary>
    Task<string> GetAccessTokenAsync(Guid lineOfficialAccountId, CancellationToken ct = default);

    /// <summary>取得屬於指定 Identity 使用者的官方帳號的 Access Token；不存在或不屬於他時回傳 null</summary>
    Task<string?> FindAccessTokenForOwnerAsync(Guid lineOfficialAccountId, string identityUserId, CancellationToken ct = default);

    /// <summary>取得屬於指定 Identity 使用者的官方帳號；不存在或不屬於他時回傳 null</summary>
    Task<LineOfficialAccount?> FindOwnedAsync(Guid lineOfficialAccountId, string identityUserId, CancellationToken ct = default);

    /// <summary>某個 Identity 使用者的所有官方帳號（依建立時間）</summary>
    Task<List<LineOfficialAccount>> ListByIdentityUserAsync(string identityUserId, CancellationToken ct = default);

    /// <summary>透過 UserProfile 取得官方帳號清單</summary>
    Task<List<LineOfficialAccount>> FindByUserProfileIdAsync(Guid userProfileId, CancellationToken ct = default);

    /// <summary>同一個使用者是不是已經綁過這個 Channel</summary>
    Task<bool> ExistsChannelAsync(Guid userProfileId, string channelId, CancellationToken ct = default);

    Task AddAsync(LineOfficialAccount account, CancellationToken ct = default);

    /// <summary>刪除官方帳號，連同它在本地的圖文選單紀錄和排程</summary>
    Task DeleteAsync(Guid lineOfficialAccountId, CancellationToken ct = default);
}
