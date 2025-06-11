using LineConsole.Application.UserProfiles.Models;

namespace LineConsole.Application.UserProfiles.Interfaces;

/// <summary>
/// 使用者服務介面，定義使用者相關的業務操作（非帳號登入）
/// </summary>
public interface IUserProfileService
{
    /// <summary>
    /// 註冊新使用者擴充資料，若輸入中包含 LINE 官方帳號資料，則一併建立
    /// </summary>
    Task<CreateUserProfileResult> RegisterAsync(CreateUserProfileInput input);

    /// <summary>
    /// 根據 Identity 使用者 ID 查詢擴充資料（不含關聯資料）
    /// </summary>
    Task<UserProfileDTO?> GetByIdentityUserIdAsync(string identityUserId);

    /// <summary>
    /// 回傳完整的使用者詳細資料（含 LINE 官方帳號等關聯資料）
    /// </summary>
    Task<UserProfileDetails?> GetDetailsByIdentityUserIdAsync(string identityUserId);

    /// <summary>
    /// 刪除指定使用者擴充資料
    /// </summary>
    Task DeleteAsync(Guid userProfileId);
}
