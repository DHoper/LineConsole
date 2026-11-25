using LineConsole.Application.RichMenus;
using LineConsole.Application.RichMenus.Models;
using LineConsole.Domain.Entities;
using LineConsole.Infrastructure.Data;
using LineConsole.Infrastructure.Data.EfEntities;
using LineConsole.Infrastructure.Data.Repositories;
using LineConsole.Infrastructure.Demo.FakeLine;
using LineConsole.Infrastructure.Security;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace LineConsole.Tests.Scheduling;

/// <summary>
/// 排程執行器：真的 EF 查詢（SQLite）＋模擬的 LINE，時間由測試指定。
/// 檢查的是「LINE 上的預設選單」最後變成什麼，也就是使用者在 LINE 上實際看到的結果。
/// </summary>
public sealed class RichMenuScheduleRunnerTests : IDisposable
{
    private static readonly DateTime T0 = new(2026, 12, 1, 2, 0, 0, DateTimeKind.Utc);
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _db;
    private readonly FakeLineClient _line = new(new FakeLineStore());
    private readonly LineOfficialAccountRepository _accounts;
    private readonly RichMenuRepository _menus;
    private readonly RichMenuScheduleRepository _schedules;
    private readonly RichMenuScheduleRunner _runner;

    public RichMenuScheduleRunnerTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options);
        _db.Database.EnsureCreated();
        _accounts = new LineOfficialAccountRepository(_db, new CredentialProtector(new EphemeralDataProtectionProvider(), NullLogger<CredentialProtector>.Instance));
        _menus = new RichMenuRepository(_db);
        _schedules = new RichMenuScheduleRepository(_db);
        _runner = new RichMenuScheduleRunner(_schedules, _menus, _accounts, _line);
    }

    private async Task<Guid> AccountAsync(string token)
    {
        var profileId = Guid.NewGuid();
        _db.UserProfiles.Add(new UserProfileEntity { Id = profileId, IdentityUserId = "u-" + profileId, DisplayName = "測試", CreatedAt = T0, UpdatedAt = T0 });
        await _db.SaveChangesAsync(Ct);
        var account = LineOfficialAccount.Create(profileId, "ch-" + profileId, "secret", token, "測試帳號");
        await _accounts.AddAsync(account, Ct);
        return account.Id;
    }

    /// <summary>在模擬的 LINE 建一個選單，也存本地紀錄；回傳（本地 ID、LINE ID）</summary>
    private async Task<(Guid Local, string Line)> MenuAsync(Guid accountId, string token, string name)
    {
        var dto = new RichMenuDTO
        {
            Name = name, ChatBarText = "選單", Size = new MenuSize { Width = 2500, Height = 843 },
            Areas = [new MenuArea { Bounds = new MenuBounds { X = 0, Y = 0, Width = 2500, Height = 843 }, Action = new MenuAction { Type = "message", Text = "hi" } }]
        };
        var lineId = (await _line.SendJsonAsync<RichMenuIdResult>("/v2/bot/richmenu", HttpMethod.Post, token, dto, ct: Ct))!.RichMenuId;
        var menu = RichMenu.Create(accountId, lineId, name, "選單", false, 2500, 843);
        await _menus.AddAsync(menu, Ct);
        return (menu.Id, lineId);
    }

    private async Task<Guid> ScheduleAsync(Guid accountId, Guid menuId, int startMin, int endMin)
    {
        var s = RichMenuSchedule.Create(accountId, menuId, T0.AddMinutes(startMin), T0.AddMinutes(endMin), T0);
        await _schedules.AddAsync(s, Ct);
        return s.Id;
    }

    private async Task<string?> DefaultAsync(string token)
    {
        try { return (await _line.GetJsonAsync<RichMenuIdResult>("/v2/bot/user/all/richmenu", token, ct: Ct))?.RichMenuId; }
        catch (LineConsole.Application.Exceptions.LineApiException) { return null; }
    }

    private Task SetDefaultAsync(string token, string lineId) =>
        _line.SendJsonAsync($"/v2/bot/user/all/richmenu/{lineId}", HttpMethod.Post, token, ct: Ct);

    private async Task<RichMenuSchedule> Reload(Guid id) => (await _schedules.FindAsync(id, Ct))!;

    [Fact]
    public async Task 到開始時間_換成排程的選單_到結束時間_還原原本的預設()
    {
        var account = await AccountAsync("t1");
        var main = await MenuAsync(account, "t1", "主選單");
        var sale = await MenuAsync(account, "t1", "特賣選單");
        await SetDefaultAsync("t1", main.Line);
        var id = await ScheduleAsync(account, sale.Local, 10, 60);

        Assert.Equal(new RichMenuScheduleRunResult(0, 0, 0, 0), await _runner.RunDueAsync(T0.AddMinutes(9), Ct));
        Assert.Equal(main.Line, await DefaultAsync("t1"));

        Assert.Equal(1, (await _runner.RunDueAsync(T0.AddMinutes(10), Ct)).Activated);
        Assert.Equal(sale.Line, await DefaultAsync("t1"));
        Assert.Equal(main.Line, (await Reload(id)).PreviousDefaultLineRichMenuId);

        Assert.Equal(1, (await _runner.RunDueAsync(T0.AddMinutes(60), Ct)).Completed);
        Assert.Equal(main.Line, await DefaultAsync("t1"));
        Assert.Equal(RichMenuScheduleStatus.Completed, (await Reload(id)).Status);
    }

    [Fact]
    public async Task 原本沒有預設選單_結束時清掉預設()
    {
        var account = await AccountAsync("t2");
        var sale = await MenuAsync(account, "t2", "特賣選單");
        await ScheduleAsync(account, sale.Local, 10, 60);

        await _runner.RunDueAsync(T0.AddMinutes(10), Ct);
        Assert.Equal(sale.Line, await DefaultAsync("t2"));
        await _runner.RunDueAsync(T0.AddMinutes(60), Ct);
        Assert.Null(await DefaultAsync("t2"));
    }

    [Fact]
    public async Task 中途被手動換掉_結束時尊重手動設定()
    {
        var account = await AccountAsync("t3");
        var main = await MenuAsync(account, "t3", "主選單");
        var sale = await MenuAsync(account, "t3", "特賣選單");
        var manual = await MenuAsync(account, "t3", "臨時選單");
        await SetDefaultAsync("t3", main.Line);
        await ScheduleAsync(account, sale.Local, 10, 60);

        await _runner.RunDueAsync(T0.AddMinutes(10), Ct);
        await SetDefaultAsync("t3", manual.Line);   // 店家在排程中途手動換了
        await _runner.RunDueAsync(T0.AddMinutes(60), Ct);

        Assert.Equal(manual.Line, await DefaultAsync("t3"));
    }

    [Fact]
    public async Task 整段時間都沒執行到_跳過不補做()
    {
        var account = await AccountAsync("t4");
        var main = await MenuAsync(account, "t4", "主選單");
        var sale = await MenuAsync(account, "t4", "特賣選單");
        await SetDefaultAsync("t4", main.Line);
        var id = await ScheduleAsync(account, sale.Local, 10, 60);

        var result = await _runner.RunDueAsync(T0.AddMinutes(90), Ct);   // 伺服器停機，第一次跑已經過了結束時間

        Assert.Equal(1, result.Skipped);
        Assert.Equal(main.Line, await DefaultAsync("t4"));
        Assert.Equal(RichMenuScheduleStatus.Skipped, (await Reload(id)).Status);
    }

    [Fact]
    public async Task LINE拒絕_這個排程標成失敗並記下原因_不影響其他帳號的排程()
    {
        var bad = await AccountAsync("invalid-token");
        var badMenuId = Guid.NewGuid();
        await _menus.AddAsync(new RichMenu { Id = badMenuId, LineOfficialAccountId = bad, LineRichMenuId = "richmenu-0123", Name = "失效帳號的選單", ChatBarText = "x", Width = 2500, Height = 843, CreatedAt = T0, UpdatedAt = T0 }, Ct);
        var badId = await ScheduleAsync(bad, badMenuId, 10, 60);

        var good = await AccountAsync("t5");
        var sale = await MenuAsync(good, "t5", "特賣選單");
        await ScheduleAsync(good, sale.Local, 10, 60);

        var result = await _runner.RunDueAsync(T0.AddMinutes(10), Ct);

        Assert.Equal(1, result.Failed);
        Assert.Equal(1, result.Activated);
        var failed = await Reload(badId);
        Assert.Equal(RichMenuScheduleStatus.Failed, failed.Status);
        Assert.Contains("LINE API", failed.LastError);
        Assert.Equal(sale.Line, await DefaultAsync("t5"));
    }

    [Fact]
    public async Task 同一個時間重複執行_不會重複處理()
    {
        var account = await AccountAsync("t6");
        var sale = await MenuAsync(account, "t6", "特賣選單");
        await ScheduleAsync(account, sale.Local, 10, 60);

        Assert.Equal(1, (await _runner.RunDueAsync(T0.AddMinutes(10), Ct)).Activated);
        Assert.False((await _runner.RunDueAsync(T0.AddMinutes(10), Ct)).AnyChange);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}
