using System;

namespace LineConsole.Domain.Entities
{
    /// <summary>
    /// Rich Menu 點擊區域實體，對應於單一 Rich Menu 的互動區塊
    /// </summary>
    public class RichMenuArea
    {
        /// <summary>點擊區域主鍵 ID</summary>
        public Guid Id { get; init; }

        /// <summary>所屬 Rich Menu ID</summary>
        public Guid RichMenuId { get; init; }

        /// <summary>點擊區塊起始 X 座標</summary>
        public int X { get; init; }

        /// <summary>點擊區塊起始 Y 座標</summary>
        public int Y { get; init; }

        /// <summary>點擊區塊寬度（像素）</summary>
        public int Width { get; init; }

        /// <summary>點擊區塊高度（像素）</summary>
        public int Height { get; init; }

        /// <summary>動作類型，例如 postback、uri、message 等</summary>
        public string ActionType { get; init; } = string.Empty;

        /// <summary>postback 或 clipboard 使用的資料</summary>
        public string? ActionData { get; init; }

        /// <summary>message action 使用的文字</summary>
        public string? ActionText { get; init; }

        /// <summary>uri action 使用的網址</summary>
        public string? ActionUri { get; init; }

        /// <summary>richmenuswitch 使用的 alias ID</summary>
        public string? RichMenuAliasId { get; init; }

        /// <summary>datetimepicker 使用的 ISO 8601 時間</summary>
        public string? DateTimeValue { get; init; }

        /// <summary>
        /// 建立新點擊區域實體
        /// </summary>
        public static RichMenuArea Create(
            Guid richMenuId,
            int x,
            int y,
            int width,
            int height,
            string actionType,
            string? actionData = null,
            string? actionText = null,
            string? actionUri = null,
            string? richMenuAliasId = null,
            string? dateTimeValue = null)
        {
            return new RichMenuArea
            {
                Id = Guid.NewGuid(),
                RichMenuId = richMenuId,
                X = x,
                Y = y,
                Width = width,
                Height = height,
                ActionType = actionType,
                ActionData = actionData,
                ActionText = actionText,
                ActionUri = actionUri,
                RichMenuAliasId = richMenuAliasId,
                DateTimeValue = dateTimeValue
            };
        }
    }
}