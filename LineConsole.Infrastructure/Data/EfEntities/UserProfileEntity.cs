﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LineConsole.Infrastructure.Data.EfEntities;

/// <summary>EF Core 映射用的使用者擴充資料表實體</summary>
[Table("user_profiles")]
public class UserProfileEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } // 主鍵 ID

    [Required]
    [MaxLength(450)]
    [Column("identity_user_id")]
    public string IdentityUserId { get; set; } = string.Empty; // 對應 Identity 使用者主鍵 ID

    [MaxLength(100)]
    [Column("display_name")]
    public string DisplayName { get; set; } = string.Empty; // 顯示名稱（可選）

    [MaxLength(512)]
    [Column("avatar_url")]
    public string? AvatarUrl { get; set; } // 大頭貼網址（可選）

    [MaxLength(100)]
    [Column("organization_code")]
    public string? OrganizationCode { get; set; } // 組織代碼或租戶識別碼（可選）

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } // 建立時間

    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } // 最後更新時間

    // 導覽屬性（optional，可略）
    public virtual ICollection<LineOfficialAccountEntity> LineOfficialAccounts { get; set; } = new List<LineOfficialAccountEntity>();
}
