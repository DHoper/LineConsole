using LineConsole.Application.LineOfficialAccounts.Interfaces;
using LineConsole.Domain.Entities;
using LineConsole.Infrastructure.Data.EfEntities;
using LineConsole.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace LineConsole.Infrastructure.Data.Repositories;

/// <summary>
/// 官方帳號的資料存取。憑證在資料庫裡是加密的：寫入前加密、讀出後解密，領域層和應用層拿到的都是明文。
/// </summary>
public class LineOfficialAccountRepository : ILineOfficialAccountRepository
{
    private readonly ApplicationDbContext _db;
    private readonly CredentialProtector _protector;

    public LineOfficialAccountRepository(ApplicationDbContext db, CredentialProtector protector)
    {
        _db = db;
        _protector = protector;
    }

    public async Task<string> GetAccessTokenAsync(Guid lineOfficialAccountId, CancellationToken ct = default)
    {
        var token = await _db.LineOfficialAccounts
            .Where(x => x.Id == lineOfficialAccountId)
            .Select(x => x.ChannelAccessToken)
            .FirstOrDefaultAsync(ct);

        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException($"找不到對應的 LINE 官方帳號或 Access Token：{lineOfficialAccountId}");

        return _protector.Unprotect(token);
    }

    public async Task<string?> FindAccessTokenForOwnerAsync(Guid lineOfficialAccountId, string identityUserId, CancellationToken ct = default)
    {
        var token = await Owned(lineOfficialAccountId, identityUserId)
            .Select(x => x.ChannelAccessToken)
            .FirstOrDefaultAsync(ct);
        return token is null ? null : _protector.Unprotect(token);
    }

    public async Task<LineOfficialAccount?> FindOwnedAsync(Guid lineOfficialAccountId, string identityUserId, CancellationToken ct = default)
    {
        var entity = await Owned(lineOfficialAccountId, identityUserId).AsNoTracking().FirstOrDefaultAsync(ct);
        return entity is null ? null : ToDomain(entity);
    }

    public async Task<List<LineOfficialAccount>> ListByIdentityUserAsync(string identityUserId, CancellationToken ct = default)
    {
        var entities = await _db.LineOfficialAccounts
            .AsNoTracking()
            .Where(x => x.UserProfile!.IdentityUserId == identityUserId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);
        return entities.Select(ToDomain).ToList();
    }

    public async Task<List<LineOfficialAccount>> FindByUserProfileIdAsync(Guid userProfileId, CancellationToken ct = default)
    {
        var entities = await _db.LineOfficialAccounts
            .AsNoTracking()
            .Where(x => x.UserProfileId == userProfileId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);
        return entities.Select(ToDomain).ToList();
    }

    public Task<bool> ExistsChannelAsync(Guid userProfileId, string channelId, CancellationToken ct = default) =>
        _db.LineOfficialAccounts.AnyAsync(x => x.UserProfileId == userProfileId && x.ChannelId == channelId, ct);

    public async Task AddAsync(LineOfficialAccount account, CancellationToken ct = default)
    {
        _db.LineOfficialAccounts.Add(new LineOfficialAccountEntity
        {
            Id = account.Id,
            UserProfileId = account.UserProfileId,
            ChannelId = account.ChannelId,
            ChannelName = account.ChannelName,
            BasicId = account.BasicId,
            ChannelSecret = _protector.Protect(account.ChannelSecret),
            ChannelAccessToken = _protector.Protect(account.ChannelAccessToken),
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt
        });
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid lineOfficialAccountId, CancellationToken ct = default)
    {
        var menuIds = await _db.RichMenus
            .Where(m => m.LineOfficialAccountId == lineOfficialAccountId)
            .Select(m => m.Id)
            .ToListAsync(ct);

        _db.RichMenuSchedules.RemoveRange(_db.RichMenuSchedules.Where(s => s.AccountId == lineOfficialAccountId || menuIds.Contains(s.RichMenuId)));
        _db.RichMenuAreas.RemoveRange(_db.RichMenuAreas.Where(a => menuIds.Contains(a.RichMenuId)));
        _db.RichMenus.RemoveRange(_db.RichMenus.Where(m => m.LineOfficialAccountId == lineOfficialAccountId));
        _db.LineOfficialAccounts.RemoveRange(_db.LineOfficialAccounts.Where(a => a.Id == lineOfficialAccountId));
        await _db.SaveChangesAsync(ct);
    }

    private IQueryable<LineOfficialAccountEntity> Owned(Guid id, string identityUserId) =>
        _db.LineOfficialAccounts.Where(x => x.Id == id && x.UserProfile!.IdentityUserId == identityUserId);

    private LineOfficialAccount ToDomain(LineOfficialAccountEntity e)
    {
        var account = LineOfficialAccount.Load(
            id: e.Id,
            userProfileId: e.UserProfileId,
            channelId: e.ChannelId,
            channelSecret: _protector.Unprotect(e.ChannelSecret),
            channelAccessToken: _protector.Unprotect(e.ChannelAccessToken),
            channelName: e.ChannelName,
            createdAt: e.CreatedAt,
            updatedAt: e.UpdatedAt);
        account.BasicId = e.BasicId;
        return account;
    }
}
