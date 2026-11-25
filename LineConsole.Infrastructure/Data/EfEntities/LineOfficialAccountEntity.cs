using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LineConsole.Infrastructure.Data.EfEntities;

/// <summary>對應 LINE 官方帳號資料表（line_official_accounts）</summary>
[Table("line_official_accounts")]
public class LineOfficialAccountEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } // 官方帳號主鍵 ID

    [Required]
    [Column("user_profile_id")]
    public Guid UserProfileId { get; set; } // 綁定的使用者個人資料 ID（對應 user_profiles.id）

    [Required]
    [MaxLength(100)]
    [Column("channel_id")]
    public string ChannelId { get; set; } = string.Empty; // LINE Channel ID（唯一識別）

    [MaxLength(100)]
    [Column("channel_name")]
    public string ChannelName { get; set; } = string.Empty; // LINE 官方帳號名稱（顯示用）

    [Required]
    [Column("channel_secret")]
    public string ChannelSecret { get; set; } = string.Empty; // 用於 webhook 驗證的 Channel Secret（Data Protection 加密後存放）

    [Required]
    [Column("channel_access_token")]
    public string ChannelAccessToken { get; set; } = string.Empty; // 呼叫 LINE API 的 Access Token（Data Protection 加密後存放）

    [MaxLength(50)]
    [Column("basic_id")]
    public string? BasicId { get; set; } // LINE 的 @ID

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } // 建立時間

    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } // 最後更新時間

    [ForeignKey("UserProfileId")]
    public virtual UserProfileEntity? UserProfile { get; set; } // 所屬後台使用者（optional）
}
