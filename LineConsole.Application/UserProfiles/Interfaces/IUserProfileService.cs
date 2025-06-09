using LineConsole.Application.Users.Models;

namespace LineConsole.Application.Users.Interfaces;

/// <summary>
/// �ϥΪ̪A�Ȥ����A�w�q�ϥΪ̬������~�Ⱦާ@�]�D�b���n�J�^
/// </summary>
public interface IUserProfileService
{
    /// <summary>���U�s�ϥΪ��X�R��ơ]�����p IdentityUser�^</summary>
    Task<CreateUserProfileResult> RegisterAsync(CreateUserProfileRequest request);

    /// <summary>�ھ� Identity �ϥΪ� ID �d���X�R���</summary>
    Task<UserProfileDTO?> GetByIdentityUserIdAsync(string identityUserId);

    /// <summary>�R�����w�ϥΪ��X�R���</summary>
    Task DeleteAsync(Guid userProfileId);
}
