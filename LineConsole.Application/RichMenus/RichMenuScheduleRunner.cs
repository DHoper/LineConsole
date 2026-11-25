using System.Net;
using LineConsole.Application.Exceptions;
using LineConsole.Application.Infrastructure.Interfaces;
using LineConsole.Application.LineOfficialAccounts.Interfaces;
using LineConsole.Application.RichMenus.Interfaces;
using LineConsole.Application.RichMenus.Models;
using LineConsole.Domain.Entities;

namespace LineConsole.Application.RichMenus;

public interface IRichMenuScheduleRunner
{
    /// <summary>處理所有到期的排程。時間由呼叫的人傳入（背景服務傳現在，測試傳任意時間）</summary>
    Task<RichMenuScheduleRunResult> RunDueAsync(DateTime nowUtc, CancellationToken ct = default);
}

/// <summary>
/// 執行到期的排程（背景服務定時呼叫，沒有使用者）：
/// - 開始：記下 LINE 上原本的預設選單，再把排程的選單設成預設。
/// - 結束：如果預設選單還是排程設的那個，就還原成原本的；中途被人手動換掉的話，尊重手動的設定，不去動它。
/// - 整段時間都錯過（例如停機）：跳過，不補做。
/// - 呼叫 LINE 失敗：這個排程標成失敗、記下原因，不影響其他排程。
/// 每個排程各自處理、各自存檔，所以重複執行也安全（已經處理過的狀態就不是到期了）。
/// </summary>
public class RichMenuScheduleRunner : IRichMenuScheduleRunner
{
    private const string DefaultMenuEndpoint = "/v2/bot/user/all/richmenu";

    private readonly IRichMenuScheduleRepository _schedules;
    private readonly IRichMenuRepository _menus;
    private readonly ILineOfficialAccountRepository _accounts;
    private readonly ILineClient _line;

    public RichMenuScheduleRunner(IRichMenuScheduleRepository schedules, IRichMenuRepository menus, ILineOfficialAccountRepository accounts, ILineClient line)
    {
        _schedules = schedules;
        _menus = menus;
        _accounts = accounts;
        _line = line;
    }

    public async Task<RichMenuScheduleRunResult> RunDueAsync(DateTime nowUtc, CancellationToken ct = default)
    {
        int activated = 0, completed = 0, skipped = 0, failed = 0;

        foreach (var schedule in await _schedules.ListDueAsync(nowUtc, ct))
        {
            try
            {
                if (schedule.WasMissed(nowUtc))
                {
                    schedule.Skip(nowUtc);
                    skipped++;
                }
                else
                {
                    var menu = await _menus.GetByIdAsync(schedule.RichMenuId, ct)
                               ?? throw new InvalidOperationException("圖文選單已經被刪除");
                    var token = await _accounts.GetAccessTokenAsync(schedule.LineOfficialAccountId, ct);

                    if (schedule.IsDueToStart(nowUtc))
                    {
                        var previous = await GetDefaultAsync(token, ct);
                        await _line.SendJsonAsync($"{DefaultMenuEndpoint}/{menu.LineRichMenuId}", HttpMethod.Post, token, ct: ct);
                        schedule.Activate(previous, nowUtc);
                        activated++;
                    }
                    else if (schedule.IsDueToEnd(nowUtc))
                    {
                        await RestoreAsync(schedule, menu, token, ct);
                        schedule.Complete(nowUtc);
                        completed++;
                    }
                }
            }
            catch (Exception ex) when (ex is LineApiException or InvalidOperationException)
            {
                var reason = ex is LineApiException line ? $"LINE API 錯誤：{line.ParsedMessage ?? line.Message}" : ex.Message;
                schedule.Fail(reason, nowUtc);
                failed++;
            }

            await _schedules.UpdateAsync(schedule, ct);
        }

        return new RichMenuScheduleRunResult(activated, completed, skipped, failed);
    }

    /// <summary>結束：預設選單還是我們設的才還原；原本沒有預設、或原本的選單已經被刪掉，就清掉預設</summary>
    private async Task RestoreAsync(RichMenuSchedule schedule, RichMenu menu, string token, CancellationToken ct)
    {
        var current = await GetDefaultAsync(token, ct);
        if (current != menu.LineRichMenuId)
            return;   // 中途被手動換掉：尊重手動設定

        var previous = schedule.PreviousDefaultLineRichMenuId;
        if (previous == menu.LineRichMenuId)
            return;   // 排程前它本來就是預設：維持原狀

        if (previous is not null)
        {
            try
            {
                await _line.SendJsonAsync($"{DefaultMenuEndpoint}/{previous}", HttpMethod.Post, token, ct: ct);
                return;
            }
            catch (LineApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // 原本的預設選單已經被刪掉：改成清掉預設
            }
        }

        await _line.DeleteAsync(DefaultMenuEndpoint, token, ct);
    }

    private async Task<string?> GetDefaultAsync(string token, CancellationToken ct)
    {
        try
        {
            return (await _line.GetJsonAsync<RichMenuIdResult>(DefaultMenuEndpoint, token, ct: ct))?.RichMenuId;
        }
        catch (LineApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;   // 沒有設定預設選單
        }
    }
}
