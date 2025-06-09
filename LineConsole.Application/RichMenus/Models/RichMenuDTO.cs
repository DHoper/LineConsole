namespace LineConsole.Application.RichMenus.Models;

/// <summary>
/// 建立或驗證 Rich Menu 結構用的資料模型（傳給 LINE API）
/// </summary>
public record RichMenuDTO
{
    public MenuSize Size { get; init; } = default!; // 選單尺寸（寬 x 高）

    public bool Selected { get; init; } // 是否為預設選單

    public string Name { get; init; } = string.Empty; // 選單名稱

    public string ChatBarText { get; init; } = string.Empty; // Chat bar 提示文字

    public List<MenuArea> Areas { get; init; } = new(); // 點擊區域清單

    public DateTime? StartTime { get; init; } // 排程起始時間
    public DateTime? EndTime { get; init; }   // 排程結束時間

}

/// <summary>
/// Rich Menu 尺寸資訊
/// </summary>
public record MenuSize
{
    public int Width { get; init; } // 寬度

    public int Height { get; init; } // 高度
}

/// <summary>
/// 點擊區域區塊定義
/// </summary>
public record MenuArea
{
    public MenuBounds Bounds { get; init; } = default!; // 點擊座標範圍

    public MenuAction Action { get; init; } = default!; // 點擊後執行的行為
}

/// <summary>
/// 點擊區塊座標範圍
/// </summary>
public record MenuBounds
{
    public int X { get; init; } // 左上角 X 座標

    public int Y { get; init; } // 左上角 Y 座標

    public int Width { get; init; } // 寬度

    public int Height { get; init; } // 高度
}

/// <summary>
/// 點擊行為定義（依 LINE API 規格）
/// </summary>
public record MenuAction
{
    public string Type { get; init; } = string.Empty; // 行為類型（如 postback、uri、message 等）

    public string? Text { get; init; } // 若為 message，所要顯示的文字

    public string? Data { get; init; } // 若為 postback，所要送出的 data

    public string? Uri { get; init; } // 若為 uri，開啟的網址

    public string? RichMenuAliasId { get; init; } // 若為 richmenuswitch，切換的別名 ID

    public string? Datetime { get; init; } // 若為 datetimepicker，ISO 格式時間字串
}
