using LineConsole.Domain.Entities;

namespace LineConsole.Application.Users.Interfaces;

/// <summary>
/// 使用者擴充資料的存取介面
/// </summary>
public interface IUserProfileRepository
{
    /// <summary>
    /// 新增 UserProfile 實體
    /// </summary>
    Task AddAsync(UserProfile user);

    /// <summary>
    /// 根據 IdentityUser 的主鍵 ID 查詢 UserProfile
    /// </summary>
    Task<UserProfile?> FindByIdentityUserIdAsync(string identityUserId);

    /// <summary>
    /// 根據 UserProfile 的 ID 刪除資料
    /// </summary>
    Task DeleteAsync(Guid userProfileId);
}
