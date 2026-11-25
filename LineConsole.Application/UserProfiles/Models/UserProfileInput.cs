
namespace LineConsole.Application.UserProfiles.Models;

/// <summary>
/// 建立使用者擴充資料的請求
/// </summary>
public class CreateUserProfileInput
{
    public string IdentityUserId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? OrganizationCode { get; set; }

}
