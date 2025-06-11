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
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>若註冊時需綁定 LINE 官方帳號，填寫此欄位</summary>
    public LineOfficialAccountCreateModel LineAccount { get; set; } = new();
}


public record class LoginInput
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

/// <summary>
/// 登入成功後回傳的資料
/// </summary>
public record class LoginResult
{
    /// <summary>JWT Token</summary>
    public string Token { get; init; } = string.Empty;

    /// <summary>Token 的過期時間（Unix timestamp，單位：秒）</summary>
    public long ExpiresAt { get; init; } 

    /// <summary>使用者資訊</summary>
    public LoginUserInfo User { get; init; } = new();
}


/// <summary>
/// 使用者基本資訊（含綁定的 LINE 官方帳號）
/// </summary>
public record class LoginUserInfo
{
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public List<LineOfficialAccountViewModel> LineAccounts { get; init; } = new();
}

