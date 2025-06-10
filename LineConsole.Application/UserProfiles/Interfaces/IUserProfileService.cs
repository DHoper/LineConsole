namespace LineConsole.Application.UserProfiles.Interfaces;

using LineConsole.Application.UserProfiles.Models;

/// <summary>
/// 使用者服務介面，定義使用者相關的業務操作（非帳號登入）
/// </summary>
public interface IUserProfileService
{
    /// <summary>
    /// 註冊新使用者擴充資料，若輸入中包含 LINE 官方帳號資料，則一併建立
    /// </summary>
    Task<CreateUserProfileResult> RegisterAsync(CreateUserProfileInput input);

    /// <summary>根據 Identity 使用者 ID 查詢擴充資料</summary>
    Task<UserProfileDTO?> GetByIdentityUserIdAsync(string identityUserId);

    /// <summary>刪除指定使用者擴充資料</summary>
    Task DeleteAsync(Guid userProfileId);
}
