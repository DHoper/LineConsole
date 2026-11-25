using LineConsole.Application.Webhooks;
using LineConsole.Domain.Entities;
using LineConsole.Infrastructure.Data.EfEntities;
using Microsoft.EntityFrameworkCore;

namespace LineConsole.Infrastructure.Data.Repositories;

public class WebhookInboxRepository : IWebhookInboxRepository
{
    private readonly ApplicationDbContext _db;

    public WebhookInboxRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// 去重兩道：先查有沒有存過（一般的重送）；
    /// 兩個請求同時送來同一個事件時，靠資料庫的唯一索引擋住第二筆（插入失敗就當成重複）。
    /// </summary>
    public async Task<bool> TryAddAsync(LineWebhookEvent e, CancellationToken ct = default)
    {
        if (await _db.LineWebhookEvents.AnyAsync(x => x.AccountId == e.LineOfficialAccountId && x.WebhookEventId == e.WebhookEventId, ct))
            return false;

        var entity = new LineWebhookEventEntity
        {
            Id = e.Id,
            AccountId = e.LineOfficialAccountId,
            WebhookEventId = e.WebhookEventId,
            EventType = e.EventType,
            LineUserId = e.LineUserId,
            OccurredAt = e.OccurredAt,
            IsRedelivery = e.IsRedelivery,
            Payload = e.Payload,
            Status = e.Status,
            ReceivedAt = e.ReceivedAt
        };
        _db.LineWebhookEvents.Add(entity);
        try
        {
            await _db.SaveChangesAsync(ct);
            _db.Entry(entity).State = EntityState.Detached;   // 存完就不追蹤：一次收很多事件時，EF 不用每次存檔都掃過全部
            return true;
        }
        catch (DbUpdateException)
        {
            _db.Entry(entity).State = EntityState.Detached;
            // 唯一索引擋下的才算重複（兩個請求同時送來同一個事件）；其他資料庫錯誤照常丟出去，LINE 會再重送
            if (await _db.LineWebhookEvents.AnyAsync(x => x.AccountId == e.LineOfficialAccountId && x.WebhookEventId == e.WebhookEventId, ct))
                return false;
            throw;
        }
    }

    public async Task<List<LineWebhookEvent>> ListPendingAsync(int max, CancellationToken ct = default) =>
        (await _db.LineWebhookEvents.AsNoTracking()
            .Where(x => x.Status == LineWebhookEventStatus.Pending)
            .OrderBy(x => x.OccurredAt)
            .Take(max)
            .ToListAsync(ct))
        .Select(ToDomain).ToList();

    public async Task UpdateAsync(LineWebhookEvent e, CancellationToken ct = default)
    {
        var entity = await _db.LineWebhookEvents.FirstAsync(x => x.Id == e.Id, ct);
        entity.Status = e.Status;
        entity.ProcessedAt = e.ProcessedAt;
        entity.Error = e.Error;
        await _db.SaveChangesAsync(ct);
        _db.Entry(entity).State = EntityState.Detached;
    }

    public async Task<List<(string EventType, DateTime OccurredAt)>> ListProcessedStatusEventsAsync(Guid lineOfficialAccountId, DateTime sinceUtc, CancellationToken ct = default)
    {
        var rows = await _db.LineWebhookEvents.AsNoTracking()
            .Where(x => x.AccountId == lineOfficialAccountId && x.Status == LineWebhookEventStatus.Processed
                        && x.OccurredAt >= sinceUtc && (x.EventType == "follow" || x.EventType == "unfollow"))
            .Select(x => new { x.EventType, x.OccurredAt })
            .ToListAsync(ct);
        return rows.Select(r => (r.EventType, DateTime.SpecifyKind(r.OccurredAt, DateTimeKind.Utc))).ToList();
    }

    private static LineWebhookEvent ToDomain(LineWebhookEventEntity e) =>
        LineWebhookEvent.Load(e.Id, e.AccountId, e.WebhookEventId, e.EventType, e.LineUserId, DateTime.SpecifyKind(e.OccurredAt, DateTimeKind.Utc),
            e.IsRedelivery, e.Payload, e.Status, DateTime.SpecifyKind(e.ReceivedAt, DateTimeKind.Utc), e.ProcessedAt, e.Error);
}
