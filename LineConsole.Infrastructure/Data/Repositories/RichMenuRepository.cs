using LineConsole.Application.RichMenus.Interfaces;
using LineConsole.Domain.Entities;
using LineConsole.Infrastructure.Data;
using LineConsole.Infrastructure.Data.EfEntities;
using Microsoft.EntityFrameworkCore;

namespace LineConsole.Infrastructure.Data.Repositories;

/// <summary>
/// Rich Menu 主體與排程資料存取實作，透過 EF Core 操作資料表 line_rich_menus、line_rich_menu_areas、line_rich_menu_schedules
/// </summary>
public class RichMenuRepository : IRichMenuRepository
{
    private readonly ApplicationDbContext _db;

    public RichMenuRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(RichMenu menu, CancellationToken ct = default)
    {
        var entity = new RichMenuEntity
        {
            Id = menu.Id,
            LineOfficialAccountId = menu.LineOfficialAccountId,
            Name = menu.Name,
            ChatBarText = menu.ChatBarText,
            Selected = menu.Selected,
            Width = menu.Width,
            Height = menu.Height,
            CreatedAt = menu.CreatedAt,
            UpdatedAt = menu.UpdatedAt
        };

        _db.RichMenus.Add(entity);

        var areaEntities = menu.Areas.Select(area => new RichMenuAreaEntity
        {
            Id = area.Id,
            RichMenuId = area.RichMenuId,
            X = area.X,
            Y = area.Y,
            Width = area.Width,
            Height = area.Height,
            ActionType = area.ActionType,
            ActionText = area.ActionText,
            ActionData = area.ActionData,
            ActionUri = area.ActionUri,
            RichMenuAliasId = area.RichMenuAliasId,
            DateTimeValue = area.DateTimeValue
        });

        _db.RichMenuAreas.AddRange(areaEntities);

        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<RichMenu>> GetAllByAccountAsync(Guid lineOfficialAccountId, CancellationToken ct = default)
    {
        var menus = await _db.RichMenus
            .AsNoTracking()
            .Where(x => x.LineOfficialAccountId == lineOfficialAccountId)
            .ToListAsync(ct);

        var menuIds = menus.Select(m => m.Id).ToList();
        var areas = await _db.RichMenuAreas
            .Where(a => menuIds.Contains(a.RichMenuId))
            .ToListAsync(ct);

        return menus.Select(menu =>
        {
            var mappedAreas = areas
                .Where(a => a.RichMenuId == menu.Id)
                .Select(a => new RichMenuArea
                {
                    Id = a.Id,
                    RichMenuId = a.RichMenuId,
                    X = a.X,
                    Y = a.Y,
                    Width = a.Width,
                    Height = a.Height,
                    ActionType = a.ActionType,
                    ActionText = a.ActionText,
                    ActionData = a.ActionData,
                    ActionUri = a.ActionUri,
                    RichMenuAliasId = a.RichMenuAliasId,
                    DateTimeValue = a.DateTimeValue
                }).ToList();

            return new RichMenu
            {
                Id = menu.Id,
                LineOfficialAccountId = menu.LineOfficialAccountId,
                Name = menu.Name,
                ChatBarText = menu.ChatBarText,
                Selected = menu.Selected,
                Width = menu.Width,
                Height = menu.Height,
                CreatedAt = menu.CreatedAt,
                UpdatedAt = menu.UpdatedAt,
                Areas = mappedAreas
            };
        }).ToList();
    }

    public async Task<RichMenu?> GetByIdAsync(Guid richMenuId, CancellationToken ct = default)
    {
        var menu = await _db.RichMenus
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == richMenuId, ct);

        if (menu is null) return null;

        var areas = await _db.RichMenuAreas
            .Where(x => x.RichMenuId == richMenuId)
            .ToListAsync(ct);

        var mappedAreas = areas.Select(a => new RichMenuArea
        {
            Id = a.Id,
            RichMenuId = a.RichMenuId,
            X = a.X,
            Y = a.Y,
            Width = a.Width,
            Height = a.Height,
            ActionType = a.ActionType,
            ActionText = a.ActionText,
            ActionData = a.ActionData,
            ActionUri = a.ActionUri,
            RichMenuAliasId = a.RichMenuAliasId,
            DateTimeValue = a.DateTimeValue
        }).ToList();

        return new RichMenu
        {
            Id = menu.Id,
            LineOfficialAccountId = menu.LineOfficialAccountId,
            Name = menu.Name,
            ChatBarText = menu.ChatBarText,
            Selected = menu.Selected,
            Width = menu.Width,
            Height = menu.Height,
            CreatedAt = menu.CreatedAt,
            UpdatedAt = menu.UpdatedAt,
            Areas = mappedAreas
        };
    }

    public async Task DeleteAsync(Guid richMenuId, CancellationToken ct = default)
    {
        var areas = await _db.RichMenuAreas
            .Where(x => x.RichMenuId == richMenuId)
            .ToListAsync(ct);

        _db.RichMenuAreas.RemoveRange(areas);

        var menu = await _db.RichMenus.FindAsync(new object[] { richMenuId }, ct);
        if (menu != null)
            _db.RichMenus.Remove(menu);

        await _db.SaveChangesAsync(ct);
    }

    public async Task AddScheduleAsync(Guid lineOfficialAccountId, Guid richMenuId, DateTime startTime, DateTime endTime, CancellationToken ct = default)
    {
        var entity = new RichMenuScheduleEntity
        {
            Id = Guid.NewGuid(),
            RichMenuId = richMenuId,
            StartTime = startTime,
            EndTime = endTime,
            IsExecuted = false,
            ExecutedAt = null,
            CreatedAt = DateTime.UtcNow
        };

        _db.RichMenuSchedules.Add(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<RichMenuSchedule>> GetActiveSchedulesAsync(Guid lineOfficialAccountId, DateTime now, CancellationToken ct = default)
    {
        var richMenuIds = await _db.RichMenus
            .Where(r => r.LineOfficialAccountId == lineOfficialAccountId)
            .Select(r => r.Id)
            .ToListAsync(ct);

        var schedules = await _db.RichMenuSchedules
            .AsNoTracking()
            .Where(s =>
                richMenuIds.Contains(s.RichMenuId) &&
                !s.IsExecuted &&
                s.StartTime <= now &&
                s.EndTime >= now)
            .ToListAsync(ct);

        return schedules.Select(s => new RichMenuSchedule
        {
            Id = s.Id,
            RichMenuId = s.RichMenuId,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            IsExecuted = s.IsExecuted,
            ExecutedAt = s.ExecutedAt,
            CreatedAt = s.CreatedAt
        }).ToList();
    }
}
