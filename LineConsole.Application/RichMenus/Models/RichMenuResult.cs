namespace LineConsole.Application.RichMenus.Models;

/// <summary>回傳單一 Rich Menu 詳細資料</summary>
public record RichMenuResult
{
    public string RichMenuId { get; init; } = string.Empty; // RichMenuDTO 對應 ID

    public string Name { get; init; } = string.Empty; // 選單名稱

    public string ChatBarText { get; init; } = string.Empty; // 開啟提示文字

    public bool Selected { get; init; } // 是否為預設

    public MenuSize Size { get; init; } = default!; // 尺寸（寬高）

    public List<MenuArea> Areas { get; init; } = new(); // 點擊區塊清單
}

/// <summary>回傳 Rich Menu ID 的結果</summary>
public record RichMenuIdResult
{
    public string RichMenuId { get; init; } = string.Empty; // RichMenuDTO 對應 ID
}

/// <summary>回傳 Rich Menu 清單結果</summary>
public record RichMenuListResult
{
    public List<RichMenuResult> RichMenus { get; init; } = new(); // 所有 RichMenuDTO 結果
}

/// <summary>回傳 Rich Menu（含圖片 base64 預覽）</summary>
public record RichMenuWithImageResult
{
    public required string RichMenuId { get; init; } // RichMenuDTO 對應 ID

    public required string Name { get; init; } // 名稱

    public required int Width { get; init; } // 寬

    public required int Height { get; init; } // 高

    public required bool Selected { get; init; } // 是否為預設

    public required string ChatBarText { get; init; } // 顯示文字

    public required string ImageBase64 { get; init; } // 預覽圖 base64 字串
}
