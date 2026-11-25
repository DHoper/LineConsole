using LineConsole.Application.RichMenus.Interfaces;
using LineConsole.Domain.Entities;
using LineConsole.Infrastructure.Data.EfEntities;
using Microsoft.EntityFrameworkCore;

namespace LineConsole.Infrastructure.Data.Repositories;

/// <summary>
/// 圖文選單的本地紀錄。本地 ID（GUID）跟 LINE 的選單 ID（richmenu-…）分開存，用 LINE 的 ID 也查得到。
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
        _db.RichMenus.Add(new RichMenuEntity
        {
            Id = menu.Id,
            LineOfficialAccountId = menu.LineOfficialAccountId,
            LineRichMenuId = menu.LineRichMenuId,
            Name = menu.Name,
            ChatBarText = menu.ChatBarText,
            Selected = menu.Selected,
            Width = menu.Width,
            Height = menu.Height,
            CreatedAt = menu.CreatedAt,
            UpdatedAt = menu.UpdatedAt
        });

        _db.RichMenuAreas.AddRange(menu.Areas.Select(area => new RichMenuAreaEntity
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
        }));

        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<RichMenu>> GetAllByAccountAsync(Guid lineOfficialAccountId, CancellationToken ct = default)
    {
        var menus = await _db.RichMenus
            .AsNoTracking()
            .Include(m => m.Areas)
            .Where(x => x.LineOfficialAccountId == lineOfficialAccountId)
            .ToListAsync(ct);
        return menus.Select(ToDomain).ToList();
    }

    public async Task<RichMenu?> GetByIdAsync(Guid richMenuId, CancellationToken ct = default)
    {
        var menu = await _db.RichMenus.AsNoTracking().Include(m => m.Areas).FirstOrDefaultAsync(x => x.Id == richMenuId, ct);
        return menu is null ? null : ToDomain(menu);
    }

    public async Task<RichMenu?> FindByLineIdAsync(Guid lineOfficialAccountId, string lineRichMenuId, CancellationToken ct = default)
    {
        var menu = await _db.RichMenus.AsNoTracking().Include(m => m.Areas)
            .FirstOrDefaultAsync(x => x.LineOfficialAccountId == lineOfficialAccountId && x.LineRichMenuId == lineRichMenuId, ct);
        return menu is null ? null : ToDomain(menu);
    }

    public async Task DeleteAsync(Guid richMenuId, CancellationToken ct = default)
    {
        _db.RichMenuSchedules.RemoveRange(_db.RichMenuSchedules.Where(x => x.RichMenuId == richMenuId));
        _db.RichMenuAreas.RemoveRange(_db.RichMenuAreas.Where(x => x.RichMenuId == richMenuId));
        _db.RichMenus.RemoveRange(_db.RichMenus.Where(x => x.Id == richMenuId));
        await _db.SaveChangesAsync(ct);
    }

    private static RichMenu ToDomain(RichMenuEntity menu) => new()
    {
        Id = menu.Id,
        LineOfficialAccountId = menu.LineOfficialAccountId,
        LineRichMenuId = menu.LineRichMenuId,
        Name = menu.Name,
        ChatBarText = menu.ChatBarText,
        Selected = menu.Selected,
        Width = menu.Width,
        Height = menu.Height,
        CreatedAt = menu.CreatedAt,
        UpdatedAt = menu.UpdatedAt,
        Areas = menu.Areas.Select(a => new RichMenuArea
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
        }).ToList()
    };
}
