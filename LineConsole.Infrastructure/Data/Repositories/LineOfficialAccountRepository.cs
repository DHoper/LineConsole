using LineConsole.Application.LineOfficialAccounts.Interfaces;
using LineConsole.Domain.Entities;
using LineConsole.Infrastructure.Data.EfEntities;
using Microsoft.EntityFrameworkCore;

namespace LineConsole.Infrastructure.Data.Repositories;

/// <summary>
/// 實作 LINE 官方帳號相關資料存取邏輯，例如存取 Access Token 或建立帳號資料
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
            .Select(x => x.ChannelAccessToken)
            .FirstOrDefaultAsync(ct);

        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException($"找不到對應的 LINE 官方帳號或 Access Token：{lineOfficialAccountId}");

        return token;
    }

    public async Task AddAsync(LineOfficialAccount account, CancellationToken ct = default)
    {
        var entity = new LineOfficialAccountEntity
        {
            Id = account.Id,
            UserProfileId = account.UserProfileId,
            ChannelId = account.ChannelId,
            ChannelSecret = account.ChannelSecret,
            ChannelAccessToken = account.ChannelAccessToken,
            ChannelName = account.ChannelName,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt
        };

        _db.LineOfficialAccounts.Add(entity);
        await _db.SaveChangesAsync(ct);
    }
}
