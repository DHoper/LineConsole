namespace LineConsole.Application.LineOfficialAccounts.Interfaces;

/// <summary>
/// 官方帳號的存取控制：使用者發出的請求，一律經過這裡取得 Access Token。
/// </summary>
public interface IOfficialAccountAccess
{
    /// <summary>
    /// 取得「目前登入者擁有的」官方帳號的 Channel Access Token。
    /// 帳號不存在、或不屬於目前的使用者，都丟 <see cref="Common.Models.NotFoundException"/>。
    /// </summary>
    Task<string> GetAccessTokenAsync(Guid lineOfficialAccountId, CancellationToken ct = default);

    /// <summary>只確認帳號是目前登入者的（不需要 token 的操作用，例如讀排程）；不是就丟 NotFoundException</summary>
    Task EnsureOwnerAsync(Guid lineOfficialAccountId, CancellationToken ct = default);
}
