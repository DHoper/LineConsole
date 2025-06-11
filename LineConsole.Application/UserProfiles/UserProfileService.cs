using LineConsole.Application.UserProfiles.Interfaces;
using LineConsole.Application.UserProfiles.Models;
using LineConsole.Application.LineOfficialAccounts.Interfaces;
using LineConsole.Application.LineOfficialAccounts.Models;
using LineConsole.Domain.Entities;
using LineConsole.Application.Common.Models;
using LineConsole.Server.Models.Api;

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
        var userProfile = UserProfile.Create(
            input.IdentityUserId,
            input.DisplayName,
            input.AvatarUrl,
            input.OrganizationCode
        );
        await _userRepository.AddAsync(userProfile);

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

    /// <summary>
    /// �ھ� IdentityUserId �d�ߨϥΪ��X�R��ơ]���]�t LINE �x��b���^
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
    /// �R�����w�� UserProfile ���
    /// </summary>
    public async Task DeleteAsync(Guid userProfileId)
    {
        await _userRepository.DeleteAsync(userProfileId);
    }

    /// <summary>
    /// �^�ǧ���ϥΪ̸ԲӸ�ơ]�t LINE �x��b���M��^�A�����μh�ո˨ϥ�
    /// </summary>
    public async Task<UserProfileDetails?> GetDetailsByIdentityUserIdAsync(string identityUserId)
    {
        var user = await _userRepository.FindByIdentityUserIdAsync(identityUserId);
        if (user is null) return null;

        var accounts = await _lineAccountRepository.FindByUserProfileIdAsync(user.Id);

        return new UserProfileDetails
        {
            UserProfileId = user.Id,
            IdentityUserId = user.IdentityUserId,
            DisplayName = user.DisplayName,
            LineAccounts = accounts.Select(x => new LineOfficialAccountViewModel
            {
                Id = x.Id,
                ChannelId = x.ChannelId,
                ChannelName = x.ChannelName
            }).ToList()
        };
    }
}
