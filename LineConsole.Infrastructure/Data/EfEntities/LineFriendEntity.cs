using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LineConsole.Domain.Entities;

namespace LineConsole.Infrastructure.Data.EfEntities;

/// <summary>官方帳號的好友（同一個帳號的 LINE userId 唯一）</summary>
[Table("line_friends")]
public class LineFriendEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("account_id")]
    public Guid AccountId { get; set; }

    [Required]
    [MaxLength(64)]
    [Column("line_user_id")]
    public string LineUserId { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("display_name")]
    public string? DisplayName { get; set; }

    [MaxLength(500)]
    [Column("picture_url")]
    public string? PictureUrl { get; set; }

    [Required]
    [Column("status")]
    public LineFriendStatus Status { get; set; }

    [Required]
    [Column("followed_at")]
    public DateTime FollowedAt { get; set; }

    [Column("blocked_at")]
    public DateTime? BlockedAt { get; set; }

    [Column("last_message_at")]
    public DateTime? LastMessageAt { get; set; }

    [Required]
    [Column("last_status_event_at")]
    public DateTime LastStatusEventAt { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
