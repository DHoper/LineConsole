using LineConsole.Application.LineOfficialAccounts.Interfaces;
using LineConsole.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LineConsole.Infrastructure.Data.Repositories;

/// <summary>
/// 實作 LINE 官方帳號相關資料存取邏輯，例如存取 Access Token
/// </summary>
public class LineOfficialAccountRepository : ILineOfficialAccountRepository
{
    private readonly ApplicationDbContext _db;

    public LineOfficialAccountRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<string> GetAccessTokenAsync(Guid lineOfficialAccountId, CancellationToken ct = default)
    {
        var token = await _db.LineOfficialAccounts
            .Where(x => x.Id == lineOfficialAccountId)
            .Select(x => x.AccessToken)
            .FirstOrDefaultAsync(ct);

        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException($"找不到對應的 LINE 官方帳號或 access token：{lineOfficialAccountId}");

        return token;
    }
}
