using LineConsole.Application.UserProfiles.Interfaces;
using LineConsole.Application.UserProfiles.Models;
using LineConsole.Application.LineOfficialAccounts.Interfaces;
using LineConsole.Application.LineOfficialAccounts.Models;
using LineConsole.Domain.Entities;

namespace LineConsole.Application.UserProfiles;

/// <summary>
/// ���ѨϥΪ��X�R��ƪ����U�B�d�ߡB�R�������μh�޿�]�D�b���n�J�^
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
    /// ���U�s�� UserProfile �øj�w LINE �x��b���]IdentityUser ��إߧ�����I�s�^
    /// </summary>
    public async Task<CreateUserProfileResult> RegisterAsync(CreateUserProfileInput input)
    {
        // �إ� UserProfile
        var userProfile = UserProfile.Create(
            input.IdentityUserId,
            input.DisplayName,
            input.AvatarUrl,
            input.OrganizationCode
        );
        await _userRepository.AddAsync(userProfile);

        // �j�w LINE �x��b��
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
