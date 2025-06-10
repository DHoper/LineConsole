namespace LineConsole.Application.UserProfiles.Interfaces;

using LineConsole.Application.UserProfiles.Models;

/// <summary>
/// �ϥΪ̪A�Ȥ����A�w�q�ϥΪ̬������~�Ⱦާ@�]�D�b���n�J�^
/// </summary>
public interface IUserProfileService
{
    /// <summary>
    /// ���U�s�ϥΪ��X�R��ơA�Y��J���]�t LINE �x��b����ơA�h�@�֫إ�
    /// </summary>
    Task<CreateUserProfileResult> RegisterAsync(CreateUserProfileInput input);

    /// <summary>�ھ� Identity �ϥΪ� ID �d���X�R���</summary>
    Task<UserProfileDTO?> GetByIdentityUserIdAsync(string identityUserId);

    /// <summary>�R�����w�ϥΪ��X�R���</summary>
    Task DeleteAsync(Guid userProfileId);
}
