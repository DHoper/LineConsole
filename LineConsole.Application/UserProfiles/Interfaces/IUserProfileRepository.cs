using LineConsole.Domain.Entities;

namespace LineConsole.Application.UserProfiles.Interfaces;

/// <summary>
/// 使用者擴充資料的存取介面
/// </summary>
public interface IUserProfileRepository
{
    /// <summary>
    /// 新增 UserProfiles 實體
    /// </summary>
    Task AddAsync(UserProfile user);

    /// <summary>
    /// 根據 IdentityUser 的主鍵 ID 查詢 UserProfiles
    /// </summary>
    Task<UserProfile?> FindByIdentityUserIdAsync(string identityUserId);

    /// <summary>
    /// 根據 UserProfiles 的 ID 刪除資料
    /// </summary>
    Task DeleteAsync(Guid userProfileId);
}
