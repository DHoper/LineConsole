using LineConsole.Application.Common.Interfaces;
using LineConsole.Application.Common.Models;
using LineConsole.Application.LineOfficialAccounts.Interfaces;

namespace LineConsole.Application.LineOfficialAccounts;

/// <summary>
/// 確認官方帳號屬於目前登入的使用者，才交出 Access Token（防止帶別人的帳號 ID 越權操作）。
/// </summary>
public class OfficialAccountAccess : IOfficialAccountAccess
{
    public const string NotFoundCode = "ACCOUNT_NOT_FOUND";

    private readonly ICurrentUser _currentUser;
    private readonly ILineOfficialAccountRepository _accounts;

    public OfficialAccountAccess(ICurrentUser currentUser, ILineOfficialAccountRepository accounts)
    {
        _currentUser = currentUser;
        _accounts = accounts;
    }

    public async Task<string> GetAccessTokenAsync(Guid lineOfficialAccountId, CancellationToken ct = default)
    {
        var userId = _currentUser.IdentityUserId;
        var token = string.IsNullOrEmpty(userId)
            ? null
            : await _accounts.FindAccessTokenForOwnerAsync(lineOfficialAccountId, userId, ct);

        if (string.IsNullOrWhiteSpace(token))
            throw new NotFoundException(NotFoundCode, "找不到這個 LINE 官方帳號");

        return token;
    }

    public Task EnsureOwnerAsync(Guid lineOfficialAccountId, CancellationToken ct = default) =>
        GetAccessTokenAsync(lineOfficialAccountId, ct);
}
