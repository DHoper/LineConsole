using LineConsole.Application.LineOfficialAccounts.Interfaces;
using LineConsole.Application.RichMenus.Interfaces;
using LineConsole.Application.Webhooks;
using LineConsole.Domain.Entities;

namespace LineConsole.Application.Friends;

/// <summary>統計「每天」用的時區（預設 Asia/Taipei）：台灣的 11/25 跟 UTC 的 11/25 不是同一段時間</summary>
public sealed record ReportingTimeZone(TimeZoneInfo Zone);

public sealed record FriendItem(Guid Id, string LineUserId, string? DisplayName, string? PictureUrl, string Status,
    DateTime FollowedAt, DateTime? BlockedAt, DateTime? LastMessageAt);

public sealed record PagedResult<T>(List<T> Items, int Total, int Page, int PageSize);

/// <param name="Date">當地日期（yyyy-MM-dd）</param>
/// <param name="Following">那天結束時的追蹤人數</param>
public sealed record DailyFriendStat(string Date, int Follows, int Unfollows, int Following);

public sealed record DashboardResult
{
    public int Following { get; init; }
    public int Blocked { get; init; }
    public int FollowsToday { get; init; }
    public int UnfollowsToday { get; init; }
    /// <summary>最近 30 天（含今天），舊的在前</summary>
    public List<DailyFriendStat> Daily { get; init; } = [];
    public int RichMenuCount { get; init; }
    public int PendingSchedules { get; init; }
    public int ActiveSchedules { get; init; }
}

public interface IFriendService
{
    Task<PagedResult<FriendItem>> ListAsync(Guid lineOfficialAccountId, string? status, string? keyword, int page, int pageSize, CancellationToken ct = default);
    Task<DashboardResult> GetDashboardAsync(Guid lineOfficialAccountId, CancellationToken ct = default);
}

public class FriendService : IFriendService
{
    public const int Days = 30;

    private readonly IOfficialAccountAccess _access;
    private readonly IFriendRepository _friends;
    private readonly IWebhookInboxRepository _inbox;
    private readonly IRichMenuRepository _menus;
    private readonly IRichMenuScheduleRepository _schedules;
    private readonly TimeProvider _time;
    private readonly TimeZoneInfo _zone;

    public FriendService(IOfficialAccountAccess access, IFriendRepository friends, IWebhookInboxRepository inbox, IRichMenuRepository menus,
        IRichMenuScheduleRepository schedules, TimeProvider time, ReportingTimeZone zone)
    {
        _access = access;
        _friends = friends;
        _inbox = inbox;
        _menus = menus;
        _schedules = schedules;
        _time = time;
        _zone = zone.Zone;
    }

    public async Task<PagedResult<FriendItem>> ListAsync(Guid lineOfficialAccountId, string? status, string? keyword, int page, int pageSize, CancellationToken ct = default)
    {
        await _access.EnsureOwnerAsync(lineOfficialAccountId, ct);
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        LineFriendStatus? filter = Enum.TryParse<LineFriendStatus>(status, ignoreCase: true, out var s) && Enum.IsDefined(s) ? s : null;

        var (items, total) = await _friends.SearchAsync(lineOfficialAccountId, filter, keyword?.Trim(), (page - 1) * pageSize, pageSize, ct);
        return new PagedResult<FriendItem>(items.Select(f => new FriendItem(f.Id, f.LineUserId, f.DisplayName, f.PictureUrl, f.Status.ToString(),
            f.FollowedAt, f.BlockedAt, f.LastMessageAt)).ToList(), total, page, pageSize);
    }

    /// <summary>
    /// 每天的新增、封鎖從處理過的 Webhook 事件算；每天結束時的追蹤人數，從「現在的人數」往回扣掉之後每天的淨增加。
    /// </summary>
    public async Task<DashboardResult> GetDashboardAsync(Guid lineOfficialAccountId, CancellationToken ct = default)
    {
        await _access.EnsureOwnerAsync(lineOfficialAccountId, ct);

        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(_time.GetUtcNow().UtcDateTime, _zone));
        var firstDay = today.AddDays(-(Days - 1));
        var sinceUtc = TimeZoneInfo.ConvertTimeToUtc(firstDay.ToDateTime(TimeOnly.MinValue), _zone);

        var (following, blocked) = await _friends.CountAsync(lineOfficialAccountId, ct);
        var events = await _inbox.ListProcessedStatusEventsAsync(lineOfficialAccountId, sinceUtc, ct);
        var byDay = events
            .GroupBy(e => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(e.OccurredAt, _zone)))
            .ToDictionary(g => g.Key, g => (Follows: g.Count(x => x.EventType == "follow"), Unfollows: g.Count(x => x.EventType == "unfollow")));

        var daily = new List<DailyFriendStat>(Days);
        var running = following;   // 從今天往回推
        for (var d = today; d >= firstDay; d = d.AddDays(-1))
        {
            var (f, u) = byDay.GetValueOrDefault(d);
            daily.Add(new DailyFriendStat(d.ToString("yyyy-MM-dd"), f, u, running));
            running -= f - u;
        }
        daily.Reverse();

        var open = await _schedules.ListOpenByAccountAsync(lineOfficialAccountId, ct);
        var todayStat = daily[^1];
        return new DashboardResult
        {
            Following = following,
            Blocked = blocked,
            FollowsToday = todayStat.Follows,
            UnfollowsToday = todayStat.Unfollows,
            Daily = daily,
            RichMenuCount = (await _menus.GetAllByAccountAsync(lineOfficialAccountId, ct)).Count,
            PendingSchedules = open.Count(x => x.Status == RichMenuScheduleStatus.Pending),
            ActiveSchedules = open.Count(x => x.Status == RichMenuScheduleStatus.Active)
        };
    }
}
