using LineConsole.Application.Users.Interfaces;
using LineConsole.Application.Users.Models;
using LineConsole.Domain.Entities;

namespace LineConsole.Application.Users;

/// <summary>
/// ���ѨϥΪ��X�R��ƪ����U�B�d�ߡB�R�������μh�޿�]�D�b���n�J�^
/// </summary>
public class UserService : IUserProfileService
{
    private readonly IUserProfileRepository _userRepository;

    public UserService(IUserProfileRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// ���U�s�� UserProfile�]IdentityUser ��إߧ�����I�s�^
    /// </summary>
    public async Task<CreateUserProfileResult> RegisterAsync(CreateUserProfileRequest request)
    {
        var user = UserProfile.Create(request.IdentityUserId);
        await _userRepository.AddAsync(user);
        return new CreateUserProfileResult { Id = user.Id };
    }

    /// <summary>
    /// �ھ� IdentityUserId �d�ߨϥΪ��X�R���
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
    /// �R�����w�� UserProfile
    /// </summary>
    public async Task DeleteAsync(Guid userProfileId)
    {
        await _userRepository.DeleteAsync(userProfileId);
    }
}
