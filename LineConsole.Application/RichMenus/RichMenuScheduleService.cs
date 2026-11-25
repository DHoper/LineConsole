using LineConsole.Application.Common.Models;
using LineConsole.Application.LineOfficialAccounts.Interfaces;
using LineConsole.Application.RichMenus.Interfaces;
using LineConsole.Application.RichMenus.Models;
using LineConsole.Domain.Entities;

namespace LineConsole.Application.RichMenus;

public interface IRichMenuScheduleService
{
    Task<List<RichMenuScheduleResult>> ListAsync(Guid lineOfficialAccountId, CancellationToken ct = default);

    Task<RichMenuScheduleResult> CreateAsync(Guid lineOfficialAccountId, CreateRichMenuScheduleInput input, CancellationToken ct = default);

    /// <summary>取消還沒開始的排程</summary>
    Task CancelAsync(Guid lineOfficialAccountId, Guid scheduleId, CancellationToken ct = default);
}

/// <summary>
/// 使用者操作排程（建立、查詢、取消）。時間規則在領域模型 <see cref="RichMenuSchedule"/>；
/// 這裡負責：確認帳號是自己的、選單存在、同一個帳號的時段不重疊。
/// </summary>
public class RichMenuScheduleService : IRichMenuScheduleService
{
    private readonly IOfficialAccountAccess _access;
    private readonly IRichMenuRepository _menus;
    private readonly IRichMenuScheduleRepository _schedules;
    private readonly TimeProvider _time;

    public RichMenuScheduleService(IOfficialAccountAccess access, IRichMenuRepository menus, IRichMenuScheduleRepository schedules, TimeProvider time)
    {
        _access = access;
        _menus = menus;
        _schedules = schedules;
        _time = time;
    }

    public async Task<List<RichMenuScheduleResult>> ListAsync(Guid lineOfficialAccountId, CancellationToken ct = default)
    {
        await _access.EnsureOwnerAsync(lineOfficialAccountId, ct);
        var menus = (await _menus.GetAllByAccountAsync(lineOfficialAccountId, ct)).ToDictionary(m => m.Id);
        var schedules = await _schedules.ListByAccountAsync(lineOfficialAccountId, ct);
        return schedules.Select(s => ToResult(s, menus.GetValueOrDefault(s.RichMenuId))).ToList();
    }

    public async Task<RichMenuScheduleResult> CreateAsync(Guid lineOfficialAccountId, CreateRichMenuScheduleInput input, CancellationToken ct = default)
    {
        await _access.EnsureOwnerAsync(lineOfficialAccountId, ct);
        var menu = await _menus.FindByLineIdAsync(lineOfficialAccountId, input.RichMenuId, ct)
                   ?? throw new NotFoundException("RICH_MENU_NOT_FOUND", "找不到這個圖文選單");

        var start = input.StartTime.UtcDateTime;
        var end = input.EndTime.UtcDateTime;
        RichMenuSchedule schedule;
        try
        {
            schedule = RichMenuSchedule.Create(lineOfficialAccountId, menu.Id, start, end, _time.GetUtcNow().UtcDateTime);
        }
        catch (ArgumentException ex)
        {
            throw new AppException("SCHEDULE_INVALID", ex.Message);
        }

        var open = await _schedules.ListOpenByAccountAsync(lineOfficialAccountId, ct);
        if (open.FirstOrDefault(s => s.Overlaps(start, end)) is { } clash)
        {
            var clashMenu = await _menus.GetByIdAsync(clash.RichMenuId, ct);
            throw new AppException("SCHEDULE_OVERLAP",
                $"這個時段跟「{clashMenu?.Name ?? "其他選單"}」的排程重疊。同一個官方帳號同時只能有一個排程中的預設選單");
        }

        await _schedules.AddAsync(schedule, ct);
        return ToResult(schedule, menu);
    }

    public async Task CancelAsync(Guid lineOfficialAccountId, Guid scheduleId, CancellationToken ct = default)
    {
        await _access.EnsureOwnerAsync(lineOfficialAccountId, ct);
        var schedule = await _schedules.FindAsync(scheduleId, ct);
        if (schedule is null || schedule.LineOfficialAccountId != lineOfficialAccountId)
            throw new NotFoundException("SCHEDULE_NOT_FOUND", "找不到這個排程");

        try
        {
            schedule.Cancel(_time.GetUtcNow().UtcDateTime);
        }
        catch (InvalidOperationException)
        {
            throw new AppException("SCHEDULE_NOT_CANCELLABLE", "只有還沒開始的排程可以取消");
        }
        await _schedules.UpdateAsync(schedule, ct);
    }

    private static RichMenuScheduleResult ToResult(RichMenuSchedule s, RichMenu? menu) => new()
    {
        Id = s.Id,
        RichMenuId = menu?.LineRichMenuId ?? string.Empty,
        RichMenuName = menu?.Name ?? "（已刪除的選單）",
        StartTime = s.StartTime,
        EndTime = s.EndTime,
        Status = s.Status.ToString(),
        ActivatedAt = s.ActivatedAt,
        CompletedAt = s.CompletedAt,
        LastError = s.LastError
    };
}
