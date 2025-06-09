using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LineConsole.Infrastructure.Data.EfEntities;

/// <summary>
/// EF Core 映射用的使用者擴充資料表實體
/// </summary>
[Table("user_profiles")]
public class UserProfileEntity
{
    /// <summary>主鍵 ID</summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>對應 Identity 使用者主鍵 ID</summary>
    [Required]
    [MaxLength(450)]
    [Column("identity_user_id")]
    public string IdentityUserId { get; set; } = string.Empty;

    /// <summary>顯示名稱（可選）</summary>
    [MaxLength(100)]
    [Column("display_name")]
    public string? DisplayName { get; set; }

    /// <summary>大頭貼網址（可選）</summary>
    [MaxLength(512)]
    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }

    /// <summary>組織代碼或租戶識別碼（可選）</summary>
    [MaxLength(100)]
    [Column("organization_code")]
    public string? OrganizationCode { get; set; }

    /// <summary>建立時間</summary>
    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>最後更新時間</summary>
    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
