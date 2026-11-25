using System.ComponentModel.DataAnnotations;
using LineConsole.Application.RichMenus.Models;

namespace LineConsole.Server.Models.Requests;

/// <summary>
/// 建立 Rich Menu 並上傳圖片的請求資料（multipart/form-data）。
/// 含 IFormFile，屬於 HTTP 傳輸格式，所以放在 Server 層，不放應用層。
/// </summary>
public class CreateRichMenuRequest
{
    /// <summary>是否為預設選單</summary>
    public bool Selected { get; init; }

    /// <summary>圖文選單名稱</summary>
    [Required]
    public string Name { get; init; } = string.Empty;

    /// <summary>Chat bar 顯示文字</summary>
    [Required]
    public string ChatBarText { get; init; } = string.Empty;

    /// <summary>尺寸資訊（寬度、高度）</summary>
    public MenuSize Size { get; init; } = default!;

    /// <summary>點擊區域清單</summary>
    public List<MenuArea> Areas { get; init; } = new();

    /// <summary>圖片檔案（JPEG/PNG）</summary>
    [Required]
    public IFormFile Image { get; init; } = null!;
}
