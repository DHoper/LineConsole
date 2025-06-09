using LineConsole.Domain.Entities;

namespace LineConsole.Application.Users.Interfaces;

/// <summary>
/// �ϥΪ��X�R��ƪ��s������
/// </summary>
public interface IUserProfileRepository
{
    /// <summary>
    /// �s�W UserProfile ����
    /// </summary>
    Task AddAsync(UserProfile user);

    /// <summary>
    /// �ھ� IdentityUser ���D�� ID �d�� UserProfile
    /// </summary>
    Task<UserProfile?> FindByIdentityUserIdAsync(string identityUserId);

    /// <summary>
    /// �ھ� UserProfile �� ID �R�����
    /// </summary>
    Task DeleteAsync(Guid userProfileId);
}
