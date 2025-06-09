using System;

namespace LineConsole.Domain.Entities
{
    /// <summary>
    /// 排程發布選單實體，將指定的 Rich Menu 設定為預設於某一時間區間
    /// </summary>
    public class RichMenuSchedule
    {
        public Guid Id { get; init; }                // 排程主鍵 ID
        public Guid RichMenuId { get; init; }        // 要設定為預設的 Rich Menu ID
        public Guid OfficialAccountId { get; init; } // 對應的 LINE 官方帳號 ID
        public DateTime StartTime { get; init; }     // 排程開始時間
        public DateTime EndTime { get; init; }       // 排程結束時間
        public bool IsExecuted { get; init; }        // 是否已執行
        public DateTime? ExecutedAt { get; init; }   // 實際執行時間（若已執行）
        public DateTime CreatedAt { get; init; }     // 建立時間（UTC）

        public static RichMenuSchedule Create(
            Guid richMenuId,
            Guid officialAccountId,
            DateTime startTime,
            DateTime endTime)
        {
            var now = DateTime.UtcNow;
            return new RichMenuSchedule
            {
                Id = Guid.NewGuid(),
                RichMenuId = richMenuId,
                OfficialAccountId = officialAccountId,
                StartTime = startTime,
                EndTime = endTime,
                IsExecuted = false,
                ExecutedAt = null,
                CreatedAt = now
            };
        }

        public void MarkExecuted()
        {
            if (!IsExecuted)
            {
            }
        }
    }
}
