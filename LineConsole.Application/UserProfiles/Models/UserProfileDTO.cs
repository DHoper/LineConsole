namespace LineConsole.Application.Users.Models;

/// <summary>後台使用者擴充資料（供外部回傳）</summary>
public record class UserProfileDTO
{
    /// <summary>擴充資料主鍵 ID</summary>
    public Guid Id { get; init; }

    /// <summary>Identity 帳號 ID</summary>
    public string IdentityUserId { get; init; } = string.Empty;

    /// <summary>使用者顯示名稱（可選）</summary>
    public string? DisplayName { get; init; }

    /// <summary>使用者大頭貼網址（可選）</summary>
    public string? AvatarUrl { get; init; }

    /// <summary>組織代碼（可選）</summary>
    public string? OrganizationCode { get; init; }

    /// <summary>建立時間（UTC）</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>最後更新時間（UTC）</summary>
    public DateTime UpdatedAt { get; init; }
}
