namespace LineConsole.Application.Common.Models;

/// <summary>
/// 定義產出 JWT Token 時所需的使用者資料內容
/// </summary>
public record JwtPayload
{
    /// <summary>使用者識別碼</summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>使用者 Email</summary>
    public string Email { get; init; } = string.Empty;
     
    /// <summary>使用者所屬角色</summary>
    public string Role { get; init; } = string.Empty;
}
