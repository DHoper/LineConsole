using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LineConsole.Infrastructure.Data.EfEntities;

/// <summary>
/// 對應 LINE 官方帳號資料表（line_official_accounts）
/// </summary>
[Table("line_official_accounts")]
public class LineOfficialAccountEntity
{
    /// <summary>官方帳號主鍵 ID</summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>綁定的使用者個人資料 ID（對應 user_profiles.id）</summary>
    [Required]
    [Column("user_profile_id")]
    public Guid UserProfileId { get; set; }

    /// <summary>LINE 平台上的 userId（channel ID）</summary>
    [Required]
    [MaxLength(100)]
    [Column("line_user_id")]
    public string LineUserId { get; set; } = string.Empty;

    /// <summary>LINE 官方帳號名稱（顯示用）</summary>
    [MaxLength(100)]
    [Column("channel_name")]
    public string? ChannelName { get; set; }

    /// <summary>LINE channel access token（建議加密儲存）</summary>
    [Required]
    [Column("access_token", TypeName = "text")]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>建立時間</summary>
    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>最後更新時間</summary>
    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
