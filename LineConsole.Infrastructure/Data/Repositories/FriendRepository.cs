using LineConsole.Application.Webhooks;
using LineConsole.Domain.Entities;
using LineConsole.Infrastructure.Data.EfEntities;
using Microsoft.EntityFrameworkCore;

namespace LineConsole.Infrastructure.Data.Repositories;

public class FriendRepository : IFriendRepository
{
    private readonly ApplicationDbContext _db;

    public FriendRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<LineFriend?> FindAsync(Guid lineOfficialAccountId, string lineUserId, CancellationToken ct = default)
    {
        var e = await _db.LineFriends.AsNoTracking().FirstOrDefaultAsync(x => x.AccountId == lineOfficialAccountId && x.LineUserId == lineUserId, ct);
        return e is null ? null : ToDomain(e);
    }

    public async Task AddAsync(LineFriend f, CancellationToken ct = default)
    {
        var entity = new LineFriendEntity { Id = f.Id, AccountId = f.LineOfficialAccountId, LineUserId = f.LineUserId, CreatedAt = f.CreatedAt };
        Copy(f, entity);
        _db.LineFriends.Add(entity);
        await _db.SaveChangesAsync(ct);
        _db.Entry(entity).State = EntityState.Detached;   // 存完就不追蹤（事件處理器一批會處理上百筆）
    }

    public async Task UpdateAsync(LineFriend f, CancellationToken ct = default)
    {
        var entity = await _db.LineFriends.FirstAsync(x => x.Id == f.Id, ct);
        Copy(f, entity);
        await _db.SaveChangesAsync(ct);
        _db.Entry(entity).State = EntityState.Detached;
    }

    public async Task<(List<LineFriend> Items, int Total)> SearchAsync(Guid lineOfficialAccountId, LineFriendStatus? status, string? keyword,
        int skip, int take, CancellationToken ct = default)
    {
        var q = _db.LineFriends.AsNoTracking().Where(x => x.AccountId == lineOfficialAccountId);
        if (status is not null)
            q = q.Where(x => x.Status == status);
        if (!string.IsNullOrEmpty(keyword))
            q = q.Where(x => x.DisplayName != null && x.DisplayName.Contains(keyword));

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(x => x.FollowedAt).ThenBy(x => x.Id).Skip(skip).Take(take).ToListAsync(ct);
        return (items.Select(ToDomain).ToList(), total);
    }

    public async Task<(int Following, int Blocked)> CountAsync(Guid lineOfficialAccountId, CancellationToken ct = default)
    {
        var counts = await _db.LineFriends.AsNoTracking()
            .Where(x => x.AccountId == lineOfficialAccountId)
            .GroupBy(x => x.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync(ct);
        return (counts.FirstOrDefault(c => c.Key == LineFriendStatus.Following)?.Count ?? 0,
                counts.FirstOrDefault(c => c.Key == LineFriendStatus.Blocked)?.Count ?? 0);
    }

    private static void Copy(LineFriend f, LineFriendEntity e)
    {
        e.DisplayName = f.DisplayName;
        e.PictureUrl = f.PictureUrl;
        e.Status = f.Status;
        e.FollowedAt = f.FollowedAt;
        e.BlockedAt = f.BlockedAt;
        e.LastMessageAt = f.LastMessageAt;
        e.LastStatusEventAt = f.LastStatusEventAt;
    }

    private static DateTime Utc(DateTime t) => DateTime.SpecifyKind(t, DateTimeKind.Utc);
    private static DateTime? Utc(DateTime? t) => t is null ? null : Utc(t.Value);

    private static LineFriend ToDomain(LineFriendEntity e) =>
        LineFriend.Load(e.Id, e.AccountId, e.LineUserId, e.DisplayName, e.PictureUrl, e.Status, Utc(e.FollowedAt), Utc(e.BlockedAt),
            Utc(e.LastMessageAt), Utc(e.LastStatusEventAt), Utc(e.CreatedAt));
}
