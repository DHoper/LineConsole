namespace LineConsole.Domain.Entities;

/// <summary>
/// 後台使用者的擴充資料（與 Identity 使用者關聯）
/// </summary>
public class UserProfile
{
    /// <summary>主鍵 ID</summary>
    public Guid Id { get; init; }

    /// <summary>對應 ASP.NET Identity 的使用者 ID</summary>
    public string IdentityUserId { get; init; } = string.Empty;

    /// <summary>顯示名稱（可選）</summary>
    public string? DisplayName { get; init; }

    /// <summary>大頭貼網址（可選）</summary>
    public string? AvatarUrl { get; init; }

    /// <summary>所屬組織代碼（可選）</summary>
    public string? OrganizationCode { get; init; }

    /// <summary>建立時間</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>最後更新時間</summary>
    public DateTime UpdatedAt { get; init; }

    /// <summary>綁定的 LINE 官方帳號清單</summary>
    public List<LineOfficialAccount> LineOfficialAccounts { get; } = new();

    /// <summary>
    /// 建立新的使用者擴充資料
    /// </summary>
    public static UserProfile Create(string identityUserId)
    {
        var now = DateTime.UtcNow;
        return new UserProfile
        {
            Id = Guid.NewGuid(),
            IdentityUserId = identityUserId,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
