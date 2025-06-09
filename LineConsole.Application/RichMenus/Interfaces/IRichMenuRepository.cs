using LineConsole.Domain.Entities;

namespace LineConsole.Application.RichMenus.Interfaces;

/// <summary>
/// 提供本地 Rich Menu 與排程資料的存取操作（僅限資料庫，不含 LINE API）
/// </summary>
public interface IRichMenuRepository
{
    // === Rich Menu 主體 ===

    /// <summary>
    /// 新增一筆 Rich Menu 主體資料（包含點擊區域）
    /// </summary>
    Task AddAsync(RichMenu menu, CancellationToken ct = default);

    /// <summary>
    /// 查詢指定 LINE 官方帳號底下所有 Rich Menu 資料（包含點擊區域）
    /// </summary>
    Task<List<RichMenu>> GetAllByAccountAsync(Guid lineOfficialAccountId, CancellationToken ct = default);

    /// <summary>
    /// 根據 RichMenuId 查詢單筆資料（包含點擊區域）
    /// </summary>
    Task<RichMenu?> GetByIdAsync(Guid richMenuId, CancellationToken ct = default);

    /// <summary>
    /// 刪除指定 Rich Menu 資料（同時刪除相關區域資料）
    /// </summary>
    Task DeleteAsync(Guid richMenuId, CancellationToken ct = default);

    // === 排程（Schedule） ===

    /// <summary>
    /// 新增一筆排程資料，指定某時間段內將某 Rich Menu 設為預設選單
    /// </summary>
    Task AddScheduleAsync(Guid lineOfficialAccountId, Guid richMenuId, DateTime startTime, DateTime endTime, CancellationToken ct = default);

    /// <summary>
    /// 查詢某帳號目前尚未執行且在指定時間內的排程（用於背景排程處理）
    /// </summary>
    Task<List<RichMenuSchedule>> GetActiveSchedulesAsync(Guid lineOfficialAccountId, DateTime now, CancellationToken ct = default);
}
