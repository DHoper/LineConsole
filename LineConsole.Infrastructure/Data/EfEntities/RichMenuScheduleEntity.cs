using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LineConsole.Infrastructure.Data.EfEntities;

/// <summary>
/// 對應排程發布 Rich Menu 資料表（line_rich_menu_schedules）
/// </summary>
[Table("line_rich_menu_schedules")]
public class RichMenuScheduleEntity
{
    [ForeignKey("RichMenuId")]
    public RichMenuEntity RichMenu { get; set; } = null!;

    /// <summary>排程主鍵 ID</summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>要設定為預設的 Rich Menu ID</summary>
    [Required]
    [Column("rich_menu_id")]
    public Guid RichMenuId { get; set; }

    /// <summary>對應的 LINE 官方帳號 ID</summary>
    [Required]
    [Column("account_id")]
    public Guid AccountId { get; set; }

    /// <summary>排程開始時間</summary>
    [Required]
    [Column("start_time")]
    public DateTime StartTime { get; set; }

    /// <summary>排程結束時間</summary>
    [Required]
    [Column("end_time")]
    public DateTime EndTime { get; set; }

    /// <summary>是否已執行</summary>
    [Required]
    [Column("is_executed")]
    public bool IsExecuted { get; set; }

    /// <summary>實際執行時間（若已執行）</summary>
    [Column("executed_at")]
    public DateTime? ExecutedAt { get; set; }

    /// <summary>建立時間</summary>
    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
