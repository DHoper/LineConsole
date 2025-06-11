using LineConsole.Application.LineOfficialAccounts.Models;

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

    /// <summary>若需要立即綁定一個 LINE 官方帳號，則填入此資料</summary>
    public LineOfficialAccountCreateModel? LineAccount { get; set; }
}
