using LineConsole.Domain.Entities;

namespace LineConsole.Application.UserProfiles.Interfaces;

/// <summary>
/// �ϥΪ��X�R��ƪ��s������
/// </summary>
public interface IUserProfileRepository
{
    /// <summary>
    /// �s�W UserProfiles ����
    /// </summary>
    Task AddAsync(UserProfile user);

    /// <summary>
    /// �ھ� IdentityUser ���D�� ID �d�� UserProfiles
    /// </summary>
    Task<UserProfile?> FindByIdentityUserIdAsync(string identityUserId);

    /// <summary>
    /// �ھ� UserProfiles �� ID �R�����
    /// </summary>
    Task DeleteAsync(Guid userProfileId);
}
