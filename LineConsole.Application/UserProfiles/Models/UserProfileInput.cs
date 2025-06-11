using LineConsole.Application.LineOfficialAccounts.Models;

namespace LineConsole.Application.UserProfiles.Models;

/// <summary>
/// �إߨϥΪ��X�R��ƪ��ШD
/// </summary>
public class CreateUserProfileInput
{
    public string IdentityUserId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? OrganizationCode { get; set; }

    /// <summary>�Y�ݭn�ߧY�j�w�@�� LINE �x��b���A�h��J�����</summary>
    public LineOfficialAccountCreateModel? LineAccount { get; set; }
}
