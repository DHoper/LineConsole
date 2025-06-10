using LineConsole.Application.LineOfficialAccounts.Models;

namespace LineConsole.Application.Common.Models;

/// <summary>
/// 使用者註冊資料（用於註冊 Identity、UserProfiles 與 LINE 官方帳號）
/// </summary>
public class RegisterInput
{
    /// <summary>Email，將作為登入帳號</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>密碼</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>可選：顯示名稱</summary>
    public string? DisplayName { get; set; }

    /// <summary>若註冊時需綁定 LINE 官方帳號，填寫此欄位</summary>
    public LineOfficialAccountCreateModel LineAccount { get; set; } = new();
}


public record class LoginInput
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}