using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LineConsole.Domain.Entities;

namespace LineConsole.Infrastructure.Data.EfEntities;

/// <summary>圖文選單排程（時間都是 UTC）</summary>
[Table("line_rich_menu_schedules")]
public class RichMenuScheduleEntity
{
    [ForeignKey("RichMenuId")]
    public RichMenuEntity RichMenu { get; set; } = null!;

    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("rich_menu_id")]
    public Guid RichMenuId { get; set; }

    [Required]
    [Column("account_id")]
    public Guid AccountId { get; set; }

    [Required]
    [Column("start_time")]
    public DateTime StartTime { get; set; }

    [Required]
    [Column("end_time")]
    public DateTime EndTime { get; set; }

    [Required]
    [Column("status")]
    public RichMenuScheduleStatus Status { get; set; }

    /// <summary>開始時原本的預設選單（LINE 的 ID），結束時還原</summary>
    [MaxLength(64)]
    [Column("previous_default_line_rich_menu_id")]
    public string? PreviousDefaultLineRichMenuId { get; set; }

    [Column("activated_at")]
    public DateTime? ActivatedAt { get; set; }

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }

    [MaxLength(500)]
    [Column("last_error")]
    public string? LastError { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
