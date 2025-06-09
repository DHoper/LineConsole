using System;
using System.Collections.Generic;

namespace LineConsole.Domain.Entities
{
    /// <summary>
    /// Rich Menu 主體實體，對應於 LINE 官方帳號的圖文選單設定
    /// </summary>
    public class RichMenu
    {
        /// <summary>Rich Menu 主鍵 ID</summary>
        public Guid Id { get; init; }

        /// <summary>所屬 LINE 官方帳號 ID</summary>
        public Guid LineOfficialAccountId { get; init; }

        /// <summary>選單名稱（使用者自訂）</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>Chat bar 提示文字（顯示在聊天室下方）</summary>
        public string ChatBarText { get; init; } = string.Empty;

        /// <summary>是否為該帳號目前的預設選單</summary>
        public bool Selected { get; set; }

        /// <summary>Rich Menu 寬度（像素）</summary>
        public int Width { get; init; }

        /// <summary>Rich Menu 高度（像素）</summary>
        public int Height { get; init; }

        /// <summary>建立時間（UTC），預設為 now()</summary>
        public DateTime CreatedAt { get; init; }

        /// <summary>最後更新時間（UTC），預設為 now()</summary>
        public DateTime UpdatedAt { get; init; }

        /// <summary>此 Rich Menu 的所有點擊區域定義</summary>
        public List<RichMenuArea> Areas { get; init; } = new();

        /// <summary>
        /// 建立新 Rich Menu 實體
        /// </summary>
        public static RichMenu Create(
            Guid lineOfficialAccountId,
            string name,
            string chatBarText,
            bool selected,
            int width,
            int height)
        {
            var now = DateTime.UtcNow;
            return new RichMenu
            {
                Id = Guid.NewGuid(),
                LineOfficialAccountId = lineOfficialAccountId,
                Name = name,
                ChatBarText = chatBarText,
                Selected = selected,
                Width = width,
                Height = height,
                CreatedAt = now,
                UpdatedAt = now
            };
        }
    }
}
