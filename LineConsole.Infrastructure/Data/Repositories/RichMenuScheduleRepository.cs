using LineConsole.Application.RichMenus.Interfaces;
using LineConsole.Domain.Entities;
using LineConsole.Infrastructure.Data.EfEntities;
using Microsoft.EntityFrameworkCore;

namespace LineConsole.Infrastructure.Data.Repositories;

public class RichMenuScheduleRepository : IRichMenuScheduleRepository
{
    private readonly ApplicationDbContext _db;

    public RichMenuScheduleRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(RichMenuSchedule schedule, CancellationToken ct = default)
    {
        _db.RichMenuSchedules.Add(new RichMenuScheduleEntity
        {
            Id = schedule.Id,
            RichMenuId = schedule.RichMenuId,
            AccountId = schedule.LineOfficialAccountId,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
            Status = schedule.Status,
            CreatedAt = schedule.CreatedAt
        });
        await _db.SaveChangesAsync(ct);
    }

    public async Task<RichMenuSchedule?> FindAsync(Guid scheduleId, CancellationToken ct = default)
    {
        var e = await _db.RichMenuSchedules.AsNoTracking().FirstOrDefaultAsync(x => x.Id == scheduleId, ct);
        return e is null ? null : ToDomain(e);
    }

    public async Task<List<RichMenuSchedule>> ListByAccountAsync(Guid lineOfficialAccountId, CancellationToken ct = default) =>
        (await _db.RichMenuSchedules.AsNoTracking()
            .Where(x => x.AccountId == lineOfficialAccountId)
            .OrderByDescending(x => x.StartTime)
            .ToListAsync(ct))
        .Select(ToDomain).ToList();

    public async Task<List<RichMenuSchedule>> ListOpenByAccountAsync(Guid lineOfficialAccountId, CancellationToken ct = default) =>
        (await _db.RichMenuSchedules.AsNoTracking()
            .Where(x => x.AccountId == lineOfficialAccountId &&
                        (x.Status == RichMenuScheduleStatus.Pending || x.Status == RichMenuScheduleStatus.Active))
            .ToListAsync(ct))
        .Select(ToDomain).ToList();

    public async Task<List<RichMenuSchedule>> ListDueAsync(DateTime nowUtc, CancellationToken ct = default) =>
        (await _db.RichMenuSchedules.AsNoTracking()
            .Where(x => (x.Status == RichMenuScheduleStatus.Pending && x.StartTime <= nowUtc) ||
                        (x.Status == RichMenuScheduleStatus.Active && x.EndTime <= nowUtc))
            .OrderBy(x => x.StartTime)
            .ToListAsync(ct))
        .Select(ToDomain).ToList();

    public async Task UpdateAsync(RichMenuSchedule schedule, CancellationToken ct = default)
    {
        var e = await _db.RichMenuSchedules.FirstOrDefaultAsync(x => x.Id == schedule.Id, ct)
                ?? throw new InvalidOperationException($"找不到排程 {schedule.Id}");
        e.Status = schedule.Status;
        e.PreviousDefaultLineRichMenuId = schedule.PreviousDefaultLineRichMenuId;
        e.ActivatedAt = schedule.ActivatedAt;
        e.CompletedAt = schedule.CompletedAt;
        e.LastError = schedule.LastError;
        await _db.SaveChangesAsync(ct);
    }

    private static RichMenuSchedule ToDomain(RichMenuScheduleEntity e) =>
        RichMenuSchedule.Load(e.Id, e.AccountId, e.RichMenuId,
            DateTime.SpecifyKind(e.StartTime, DateTimeKind.Utc), DateTime.SpecifyKind(e.EndTime, DateTimeKind.Utc),
            e.Status, e.PreviousDefaultLineRichMenuId,
            Utc(e.ActivatedAt), Utc(e.CompletedAt), e.LastError, DateTime.SpecifyKind(e.CreatedAt, DateTimeKind.Utc));

    private static DateTime? Utc(DateTime? t) => t is null ? null : DateTime.SpecifyKind(t.Value, DateTimeKind.Utc);
}
