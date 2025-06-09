namespace LineConsole.Application.Users.Models;

/// <summary>註冊後台使用者擴充資料的請求資料</summary>
public record class CreateUserProfileRequest
{
    /// <summary>對應的 Identity 使用者 ID</summary>
    public string IdentityUserId { get; init; } = string.Empty;
}