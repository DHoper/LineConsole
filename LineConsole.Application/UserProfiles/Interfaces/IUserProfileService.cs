using LineConsole.Application.Users.Models;

namespace LineConsole.Application.Users.Interfaces;

/// <summary>
/// 使用者服務介面，定義使用者相關的業務操作（非帳號登入）
/// </summary>
public interface IUserProfileService
{
    /// <summary>註冊新使用者擴充資料（需關聯 IdentityUser）</summary>
    Task<CreateUserProfileResult> RegisterAsync(CreateUserProfileRequest request);

    /// <summary>根據 Identity 使用者 ID 查詢擴充資料</summary>
    Task<UserProfileDTO?> GetByIdentityUserIdAsync(string identityUserId);

    /// <summary>刪除指定使用者擴充資料</summary>
    Task DeleteAsync(Guid userProfileId);
}
