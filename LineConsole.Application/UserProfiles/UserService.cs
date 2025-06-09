using LineConsole.Application.Users.Interfaces;
using LineConsole.Application.Users.Models;
using LineConsole.Domain.Entities;

namespace LineConsole.Application.Users;

/// <summary>
/// 提供使用者擴充資料的註冊、查詢、刪除等應用層邏輯（非帳號登入）
/// </summary>
public class UserService : IUserProfileService
{
    private readonly IUserProfileRepository _userRepository;

    public UserService(IUserProfileRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// 註冊新的 UserProfile（IdentityUser 剛建立完成後呼叫）
    /// </summary>
    public async Task<CreateUserProfileResult> RegisterAsync(CreateUserProfileRequest request)
    {
        var user = UserProfile.Create(request.IdentityUserId);
        await _userRepository.AddAsync(user);
        return new CreateUserProfileResult { Id = user.Id };
    }

    /// <summary>
    /// 根據 IdentityUserId 查詢使用者擴充資料
    /// </summary>
    public async Task<UserProfileDTO?> GetByIdentityUserIdAsync(string identityUserId)
    {
        var user = await _userRepository.FindByIdentityUserIdAsync(identityUserId);
        if (user is null) return null;

        return new UserProfileDTO
        {
            Id = user.Id,
            IdentityUserId = user.IdentityUserId,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            OrganizationCode = user.OrganizationCode,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    /// <summary>
    /// 刪除指定的 UserProfile
    /// </summary>
    public async Task DeleteAsync(Guid userProfileId)
    {
        await _userRepository.DeleteAsync(userProfileId);
    }
}
