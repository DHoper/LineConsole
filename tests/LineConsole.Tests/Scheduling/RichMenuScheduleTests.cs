using LineConsole.Domain.Entities;

namespace LineConsole.Tests.Scheduling;

/// <summary>排程的領域規則（不需要資料庫、不需要 LINE）</summary>
public class RichMenuScheduleTests
{
    private static readonly DateTime Now = new(2026, 12, 1, 2, 0, 0, DateTimeKind.Utc);

    private static RichMenuSchedule Make(int startMin, int endMin) =>
        RichMenuSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), Now.AddMinutes(startMin), Now.AddMinutes(endMin), Now);

    [Fact]
    public void 結束時間要晚於開始時間()
    {
        Assert.Throws<ArgumentException>(() => Make(10, 10));
        Assert.Throws<ArgumentException>(() => Make(10, 5));
    }

    [Fact]
    public void 結束時間已經過了_不能排()
    {
        Assert.Throws<ArgumentException>(() => Make(-30, -1));
    }

    [Fact]
    public void 開始時間在過去但還沒結束_可以排_會馬上生效()
    {
        var s = Make(-5, 30);
        Assert.True(s.IsDueToStart(Now));
    }

    [Theory]
    [InlineData(0, 60, 30, 90, true)]    // 部分重疊
    [InlineData(0, 60, 10, 20, true)]    // 包在裡面
    [InlineData(0, 60, 60, 90, false)]   // 剛好接著：不算重疊
    [InlineData(0, 60, -30, 0, false)]   // 剛好接在前面
    public void 時段重疊判斷(int aStart, int aEnd, int bStart, int bEnd, bool overlaps)
    {
        var a = Make(aStart + 1, aEnd + 1);
        Assert.Equal(overlaps, a.Overlaps(Now.AddMinutes(bStart + 1), Now.AddMinutes(bEnd + 1)));
    }

    [Fact]
    public void 已經結束或取消的排程_不擋新的時段()
    {
        var s = Make(10, 60);
        s.Cancel(Now);
        Assert.False(s.Overlaps(Now.AddMinutes(10), Now.AddMinutes(60)));
    }

    [Fact]
    public void 正常流程_待執行到進行中到完成()
    {
        var s = Make(10, 60);
        Assert.False(s.IsDueToStart(Now));
        Assert.True(s.IsDueToStart(Now.AddMinutes(10)));

        s.Activate("richmenu-before", Now.AddMinutes(10));
        Assert.Equal(RichMenuScheduleStatus.Active, s.Status);
        Assert.Equal("richmenu-before", s.PreviousDefaultLineRichMenuId);
        Assert.False(s.IsDueToEnd(Now.AddMinutes(59)));
        Assert.True(s.IsDueToEnd(Now.AddMinutes(60)));

        s.Complete(Now.AddMinutes(60));
        Assert.Equal(RichMenuScheduleStatus.Completed, s.Status);
    }

    [Fact]
    public void 只有還沒開始的可以取消()
    {
        var s = Make(10, 60);
        s.Activate(null, Now.AddMinutes(10));
        Assert.Throws<InvalidOperationException>(() => s.Cancel(Now.AddMinutes(11)));
    }

    [Fact]
    public void 整段時間都錯過_算錯過不是到期開始()
    {
        var s = Make(10, 60);
        Assert.True(s.WasMissed(Now.AddMinutes(61)));
        s.Skip(Now.AddMinutes(61));
        Assert.Equal(RichMenuScheduleStatus.Skipped, s.Status);
    }
}
