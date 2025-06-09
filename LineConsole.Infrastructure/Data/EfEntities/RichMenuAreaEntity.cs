using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LineConsole.Infrastructure.Data.EfEntities;

/// <summary>
/// 對應 Rich Menu 點擊區域資料表（line_rich_menu_areas）
/// </summary>
[Table("line_rich_menu_areas")]
public class RichMenuAreaEntity

{

    /// <summary>所屬 Rich Menu 導覽屬性</summary>
    [ForeignKey(nameof(RichMenuId))]
    public RichMenuEntity RichMenu { get; set; } = null!;

    /// <summary>點擊區域主鍵 ID</summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>所屬 Rich Menu ID</summary>
    [Required]
    [Column("rich_menu_id")]
    public Guid RichMenuId { get; set; }

    /// <summary>起始 X 座標</summary>
    [Required]
    [Column("x")]
    public int X { get; set; }

    /// <summary>起始 Y 座標</summary>
    [Required]
    [Column("y")]
    public int Y { get; set; }

    /// <summary>點擊區塊寬度</summary>
    [Required]
    [Column("width")]
    public int Width { get; set; }

    /// <summary>點擊區塊高度</summary>
    [Required]
    [Column("height")]
    public int Height { get; set; }

    /// <summary>動作類型，例如 postback、uri、message 等</summary>
    [Required]
    [MaxLength(50)]
    [Column("action_type")]
    public string ActionType { get; set; } = string.Empty;

    /// <summary>postback 或 clipboard 用的資料</summary>
    [Column("action_data", TypeName = "text")]
    public string? ActionData { get; set; }

    /// <summary>message action 使用的文字</summary>
    [MaxLength(255)]
    [Column("action_text")]
    public string? ActionText { get; set; }

    /// <summary>uri action 使用的網址</summary>
    [Column("action_uri", TypeName = "text")]
    public string? ActionUri { get; set; }

    /// <summary>richmenuswitch 使用的 alias ID</summary>
    [MaxLength(100)]
    [Column("rich_menu_alias_id")]
    public string? RichMenuAliasId { get; set; }

    /// <summary>datetimepicker 使用的 ISO 8601 時間</summary>
    [MaxLength(50)]
    [Column("datetime_value")]
    public string? DateTimeValue { get; set; }
}
