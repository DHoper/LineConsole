using LineConsole.Application.UserProfiles.Interfaces;
using LineConsole.Application.UserProfiles.Models;
using LineConsole.Application.LineOfficialAccounts.Interfaces;
using LineConsole.Application.LineOfficialAccounts.Models;
using LineConsole.Domain.Entities;

namespace LineConsole.Application.UserProfiles;

/// <summary>
/// 提供使用者擴充資料的註冊、查詢、刪除等應用層邏輯（非帳號登入）
/// </summary>
public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _userRepository;
    private readonly ILineOfficialAccountRepository _lineAccountRepository;

    public UserProfileService(
        IUserProfileRepository userRepository,
        ILineOfficialAccountRepository lineAccountRepository)
    {
        _userRepository = userRepository;
        _lineAccountRepository = lineAccountRepository;
    }

    /// <summary>
    /// 註冊新的 UserProfile 並綁定 LINE 官方帳號（IdentityUser 剛建立完成後呼叫）
    /// </summary>
    public async Task<CreateUserProfileResult> RegisterAsync(CreateUserProfileInput input)
    {
        // 建立 UserProfile
        var userProfile = UserProfile.Create(
            input.IdentityUserId,
            input.DisplayName,
            input.AvatarUrl,
            input.OrganizationCode
        );
        await _userRepository.AddAsync(userProfile);

        // 綁定 LINE 官方帳號
        if (input.LineAccount is not null)
        {
            var lineAccount = LineOfficialAccount.Create(
                userProfileId: userProfile.Id,
                channelId: input.LineAccount.ChannelId,
                channelSecret: input.LineAccount.ChannelSecret,
                channelAccessToken: input.LineAccount.ChannelAccessToken,
                channelName: input.LineAccount.ChannelName
            );

            await _lineAccountRepository.AddAsync(lineAccount);
        }

        return new CreateUserProfileResult { Id = userProfile.Id };
    }

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

    public async Task DeleteAsync(Guid userProfileId)
    {
        await _userRepository.DeleteAsync(userProfileId);
    }
}
