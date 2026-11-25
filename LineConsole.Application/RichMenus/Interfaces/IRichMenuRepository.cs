using LineConsole.Domain.Entities;

namespace LineConsole.Application.RichMenus.Interfaces;

/// <summary>
/// 圖文選單的本地紀錄（LINE 上的選單本身由 LINE 保存，這裡記點擊區塊、建立時間、排程要用的對應）
/// </summary>
public interface IRichMenuRepository
{
    Task AddAsync(RichMenu menu, CancellationToken ct = default);

    Task<List<RichMenu>> GetAllByAccountAsync(Guid lineOfficialAccountId, CancellationToken ct = default);

    Task<RichMenu?> GetByIdAsync(Guid richMenuId, CancellationToken ct = default);

    /// <summary>用 LINE 的選單 ID（richmenu-…）找本地紀錄</summary>
    Task<RichMenu?> FindByLineIdAsync(Guid lineOfficialAccountId, string lineRichMenuId, CancellationToken ct = default);

    /// <summary>刪除本地紀錄，連同點擊區塊和排程</summary>
    Task DeleteAsync(Guid richMenuId, CancellationToken ct = default);
}
