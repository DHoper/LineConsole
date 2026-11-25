namespace LineConsole.Application.RichMenus.Models;

/// <summary>建立排程。時間帶時區（例如 2026-12-01T10:00:00+08:00），存成 UTC</summary>
public sealed record CreateRichMenuScheduleInput
{
    /// <summary>LINE 的圖文選單 ID（richmenu-…）</summary>
    public string RichMenuId { get; init; } = string.Empty;
    public DateTimeOffset StartTime { get; init; }
    public DateTimeOffset EndTime { get; init; }
}

/// <summary>排程（時間都是 UTC）</summary>
public sealed record RichMenuScheduleResult
{
    public Guid Id { get; init; }
    /// <summary>LINE 的圖文選單 ID</summary>
    public string RichMenuId { get; init; } = string.Empty;
    public string RichMenuName { get; init; } = string.Empty;
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    /// <summary>Pending、Active、Completed、Cancelled、Failed、Skipped</summary>
    public string Status { get; init; } = string.Empty;
    public DateTime? ActivatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public string? LastError { get; init; }
}

/// <summary>背景服務一次執行的結果</summary>
public sealed record RichMenuScheduleRunResult(int Activated, int Completed, int Skipped, int Failed)
{
    public bool AnyChange => Activated + Completed + Skipped + Failed > 0;
}
