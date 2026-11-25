namespace LineConsole.Domain.Entities;

/// <summary>排程的狀態</summary>
public enum RichMenuScheduleStatus
{
    /// <summary>還沒到開始時間</summary>
    Pending = 0,
    /// <summary>進行中：已經把這個選單設成預設</summary>
    Active = 1,
    /// <summary>結束：已經還原成原本的預設選單</summary>
    Completed = 2,
    /// <summary>使用者取消</summary>
    Cancelled = 3,
    /// <summary>呼叫 LINE 失敗（例如 token 失效、選單被刪掉）</summary>
    Failed = 4,
    /// <summary>錯過：整個時段裡伺服器都沒有執行到（例如停機），直接跳過，不補做</summary>
    Skipped = 5
}

/// <summary>
/// 圖文選單排程：在 StartTime 把選單設成預設，到 EndTime 還原成「排程開始前的預設選單」。
/// 狀態轉換的規則都在這裡（領域層），背景服務只負責「時間到了呼叫它、照結果去打 LINE」。
/// 時間一律是 UTC。
/// </summary>
public class RichMenuSchedule
{
    public Guid Id { get; init; }
    public Guid LineOfficialAccountId { get; init; }

    /// <summary>本地的圖文選單 ID</summary>
    public Guid RichMenuId { get; init; }

    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public RichMenuScheduleStatus Status { get; private set; }

    /// <summary>開始時 LINE 上原本的預設選單（LINE 的 ID）；結束時還原成它。原本沒有預設就是 null</summary>
    public string? PreviousDefaultLineRichMenuId { get; private set; }

    public DateTime? ActivatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? LastError { get; private set; }
    public DateTime CreatedAt { get; init; }

    /// <summary>可以排的時段：結束要晚於開始，而且結束時間還沒過</summary>
    public static RichMenuSchedule Create(Guid lineOfficialAccountId, Guid richMenuId, DateTime startUtc, DateTime endUtc, DateTime nowUtc)
    {
        if (endUtc <= startUtc)
            throw new ArgumentException("結束時間要晚於開始時間");
        if (endUtc <= nowUtc)
            throw new ArgumentException("結束時間已經過了");

        return new RichMenuSchedule
        {
            Id = Guid.NewGuid(),
            LineOfficialAccountId = lineOfficialAccountId,
            RichMenuId = richMenuId,
            StartTime = startUtc,
            EndTime = endUtc,
            Status = RichMenuScheduleStatus.Pending,
            CreatedAt = nowUtc
        };
    }

    /// <summary>從資料庫還原（不經過 Create 的檢查）</summary>
    public static RichMenuSchedule Load(Guid id, Guid accountId, Guid richMenuId, DateTime start, DateTime end, RichMenuScheduleStatus status,
        string? previousDefault, DateTime? activatedAt, DateTime? completedAt, string? lastError, DateTime createdAt)
    {
        var s = new RichMenuSchedule
        {
            Id = id, LineOfficialAccountId = accountId, RichMenuId = richMenuId, StartTime = start, EndTime = end, CreatedAt = createdAt
        };
        s.Status = status;
        s.PreviousDefaultLineRichMenuId = previousDefault;
        s.ActivatedAt = activatedAt;
        s.CompletedAt = completedAt;
        s.LastError = lastError;
        return s;
    }

    /// <summary>還在排程中（待執行或進行中）：同一個帳號的這種排程時段不能重疊</summary>
    public bool IsOpen => Status is RichMenuScheduleStatus.Pending or RichMenuScheduleStatus.Active;

    public bool Overlaps(DateTime startUtc, DateTime endUtc) => IsOpen && startUtc < EndTime && StartTime < endUtc;

    /// <summary>到了開始時間、還沒開始</summary>
    public bool IsDueToStart(DateTime nowUtc) => Status == RichMenuScheduleStatus.Pending && StartTime <= nowUtc;

    /// <summary>進行中、到了結束時間</summary>
    public bool IsDueToEnd(DateTime nowUtc) => Status == RichMenuScheduleStatus.Active && EndTime <= nowUtc;

    /// <summary>開始的時候，結束時間也已經過了（伺服器整段時間都沒跑）：不補做</summary>
    public bool WasMissed(DateTime nowUtc) => Status == RichMenuScheduleStatus.Pending && EndTime <= nowUtc;

    public void Activate(string? previousDefaultLineRichMenuId, DateTime nowUtc)
    {
        EnsureStatus(RichMenuScheduleStatus.Pending);
        PreviousDefaultLineRichMenuId = previousDefaultLineRichMenuId;
        ActivatedAt = nowUtc;
        Status = RichMenuScheduleStatus.Active;
    }

    public void Complete(DateTime nowUtc)
    {
        EnsureStatus(RichMenuScheduleStatus.Active);
        CompletedAt = nowUtc;
        Status = RichMenuScheduleStatus.Completed;
    }

    public void Skip(DateTime nowUtc)
    {
        EnsureStatus(RichMenuScheduleStatus.Pending);
        CompletedAt = nowUtc;
        LastError = "排程時段內伺服器沒有執行，已跳過";
        Status = RichMenuScheduleStatus.Skipped;
    }

    public void Fail(string error, DateTime nowUtc)
    {
        if (!IsOpen) throw new InvalidOperationException($"排程已經是 {Status}，不能標成失敗");
        LastError = error.Length > 500 ? error[..500] : error;
        CompletedAt = nowUtc;
        Status = RichMenuScheduleStatus.Failed;
    }

    /// <summary>取消：只有還沒開始的可以取消（進行中的要等它結束，或手動換預設選單）</summary>
    public void Cancel(DateTime nowUtc)
    {
        EnsureStatus(RichMenuScheduleStatus.Pending);
        CompletedAt = nowUtc;
        Status = RichMenuScheduleStatus.Cancelled;
    }

    private void EnsureStatus(RichMenuScheduleStatus expected)
    {
        if (Status != expected)
            throw new InvalidOperationException($"排程狀態是 {Status}，不是 {expected}");
    }
}
