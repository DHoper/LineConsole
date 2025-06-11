using LineConsole.Application.UserProfiles.Models;

namespace LineConsole.Application.UserProfiles.Interfaces;

/// <summary>
/// �ϥΪ̪A�Ȥ����A�w�q�ϥΪ̬������~�Ⱦާ@�]�D�b���n�J�^
/// </summary>
public interface IUserProfileService
{
    /// <summary>
    /// ���U�s�ϥΪ��X�R��ơA�Y��J���]�t LINE �x��b����ơA�h�@�֫إ�
    /// </summary>
    Task<CreateUserProfileResult> RegisterAsync(CreateUserProfileInput input);

    /// <summary>
    /// �ھ� Identity �ϥΪ� ID �d���X�R��ơ]���t���p��ơ^
    /// </summary>
    Task<UserProfileDTO?> GetByIdentityUserIdAsync(string identityUserId);

    /// <summary>
    /// �^�ǧ��㪺�ϥΪ̸ԲӸ�ơ]�t LINE �x��b�������p��ơ^
    /// </summary>
    Task<UserProfileDetails?> GetDetailsByIdentityUserIdAsync(string identityUserId);

    /// <summary>
    /// �R�����w�ϥΪ��X�R���
    /// </summary>
    Task DeleteAsync(Guid userProfileId);
}
