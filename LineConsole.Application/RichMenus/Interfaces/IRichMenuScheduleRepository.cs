using LineConsole.Domain.Entities;

namespace LineConsole.Application.RichMenus.Interfaces;

public interface IRichMenuScheduleRepository
{
    Task AddAsync(RichMenuSchedule schedule, CancellationToken ct = default);

    Task<RichMenuSchedule?> FindAsync(Guid scheduleId, CancellationToken ct = default);

    /// <summary>某個官方帳號的排程（新的在前）</summary>
    Task<List<RichMenuSchedule>> ListByAccountAsync(Guid lineOfficialAccountId, CancellationToken ct = default);

    /// <summary>還在排程中（待執行、進行中）的：檢查時段重疊用</summary>
    Task<List<RichMenuSchedule>> ListOpenByAccountAsync(Guid lineOfficialAccountId, CancellationToken ct = default);

    /// <summary>到期要處理的：待執行而且到了開始時間、或進行中而且到了結束時間（依時間先後）</summary>
    Task<List<RichMenuSchedule>> ListDueAsync(DateTime nowUtc, CancellationToken ct = default);

    /// <summary>存回狀態變化</summary>
    Task UpdateAsync(RichMenuSchedule schedule, CancellationToken ct = default);
}
