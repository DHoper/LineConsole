using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LineConsole.Infrastructure.Data.EfEntities;

/// <summary>
/// 對應 Rich Menu 主資料表（line_rich_menus）
/// </summary>
[Table("line_rich_menus")]
public class RichMenuEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>所屬 LINE 官方帳號 ID</summary>
    [Required]
    [Column("line_official_account_id")]
    public Guid LineOfficialAccountId { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("chat_bar_text")]
    public string ChatBarText { get; set; } = string.Empty;

    [Required]
    [Column("selected")]
    public bool Selected { get; set; }

    [Required]
    [Column("width")]
    public int Width { get; set; }

    [Required]
    [Column("height")]
    public int Height { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    /// <summary>此選單下的所有點擊區域</summary>
    public List<RichMenuAreaEntity> Areas { get; set; } = new();

    /// <summary>此選單的所有排程</summary>
    public List<RichMenuScheduleEntity> Schedules { get; set; } = new();
}
