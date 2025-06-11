using LineConsole.Application.LineOfficialAccounts.Models;

namespace LineConsole.Application.UserProfiles.Models;

/// <summary>
/// 使用者詳細資料（含綁定的 LINE 官方帳號清單）
/// </summary>
public record class UserProfileDetails
{
    /// <summary>UserProfile 主鍵</summary>
    public Guid UserProfileId { get; init; }

    /// <summary>IdentityUser 的 Id</summary>
    public string IdentityUserId { get; init; } = string.Empty;

    /// <summary>顯示名稱</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>綁定的 LINE 官方帳號清單</summary>
    public List<LineOfficialAccountViewModel> LineAccounts { get; init; } = new();
}
