using LineConsole.Application.LineOfficialAccounts.Models;

namespace LineConsole.Application.UserProfiles.Models;

/// <summary>
/// �ϥΪ̸ԲӸ�ơ]�t�j�w�� LINE �x��b���M��^
/// </summary>
public record class UserProfileDetails
{
    /// <summary>UserProfile �D��</summary>
    public Guid UserProfileId { get; init; }

    /// <summary>IdentityUser �� Id</summary>
    public string IdentityUserId { get; init; } = string.Empty;

    /// <summary>��ܦW��</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>�j�w�� LINE �x��b���M��</summary>
    public List<LineOfficialAccountViewModel> LineAccounts { get; init; } = new();
}
