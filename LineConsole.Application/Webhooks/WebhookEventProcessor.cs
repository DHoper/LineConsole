using LineConsole.Application.Exceptions;
using LineConsole.Application.Infrastructure.Interfaces;
using LineConsole.Application.LineOfficialAccounts.Interfaces;
using LineConsole.Domain.Entities;

namespace LineConsole.Application.Webhooks;

public interface IWebhookEventProcessor
{
    /// <summary>處理收件匣裡待處理的事件（依發生時間先後），回傳處理了幾筆</summary>
    Task<int> ProcessPendingAsync(int max = 100, CancellationToken ct = default);
}

/// <summary>
/// 處理 Webhook 事件，更新好友資料：
/// - follow：新好友（向 LINE 查暱稱、頭像）或解除封鎖。
/// - unfollow：封鎖。
/// - message：更新最後互動時間。
/// - 其他類型：標成略過，留著當紀錄。
/// 比目前狀態舊的事件不會蓋掉新的狀態（交給 <see cref="LineFriend"/> 判斷）。每筆各自存檔，一筆失敗不影響其他筆。
/// </summary>
public class WebhookEventProcessor : IWebhookEventProcessor
{
    private readonly IWebhookInboxRepository _inbox;
    private readonly IFriendRepository _friends;
    private readonly ILineOfficialAccountRepository _accounts;
    private readonly ILineClient _line;
    private readonly TimeProvider _time;

    public WebhookEventProcessor(IWebhookInboxRepository inbox, IFriendRepository friends, ILineOfficialAccountRepository accounts, ILineClient line, TimeProvider time)
    {
        _inbox = inbox;
        _friends = friends;
        _accounts = accounts;
        _line = line;
        _time = time;
    }

    public async Task<int> ProcessPendingAsync(int max = 100, CancellationToken ct = default)
    {
        var pending = await _inbox.ListPendingAsync(max, ct);
        var tokens = new Dictionary<Guid, string>();

        foreach (var e in pending)
        {
            var now = _time.GetUtcNow().UtcDateTime;
            try
            {
                if (e.EventType is not ("follow" or "unfollow" or "message"))
                {
                    e.MarkIgnored(now, "不處理的事件類型");
                }
                else if (string.IsNullOrEmpty(e.LineUserId))
                {
                    e.MarkIgnored(now, "沒有 userId（群組、聊天室的事件）");
                }
                else
                {
                    var friend = await _friends.FindAsync(e.LineOfficialAccountId, e.LineUserId, ct);
                    var isNew = friend is null;
                    friend ??= LineFriend.FirstSeen(e.LineOfficialAccountId, e.LineUserId, e.OccurredAt, now);

                    // follow／unfollow 只有「真的改變狀態」才算數（儀表板的每日新增、封鎖就是數這些）；
                    // 晚到的舊事件、重複的加好友都標成 Ignored，不會讓統計多算
                    bool counted;
                    switch (e.EventType)
                    {
                        case "follow": counted = friend.Follow(e.OccurredAt); break;
                        case "unfollow": counted = friend.Block(e.OccurredAt); break;
                        default: friend.Message(e.OccurredAt); counted = true; break;
                    }

                    if (isNew || (e.EventType == "follow" && friend.DisplayName is null))
                        await FillProfileAsync(friend, tokens, ct);

                    if (isNew) await _friends.AddAsync(friend, ct);
                    else await _friends.UpdateAsync(friend, ct);

                    if (counted) e.MarkProcessed(now);
                    else e.MarkIgnored(now, "狀態沒有改變（過時或重複的事件）");
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                e.MarkFailed(ex.Message, now);
            }

            await _inbox.UpdateAsync(e, ct);
        }

        return pending.Count;
    }

    /// <summary>向 LINE 查暱稱、頭像。查不到（使用者已封鎖、token 失效）不算失敗，只是沒有名稱</summary>
    private async Task FillProfileAsync(LineFriend friend, Dictionary<Guid, string> tokens, CancellationToken ct)
    {
        try
        {
            if (!tokens.TryGetValue(friend.LineOfficialAccountId, out var token))
                tokens[friend.LineOfficialAccountId] = token = await _accounts.GetAccessTokenAsync(friend.LineOfficialAccountId, ct);

            var profile = await _line.GetJsonAsync<LineUserProfile>($"/v2/bot/profile/{friend.LineUserId}", token, ct: ct);
            if (profile is not null)
            {
                friend.DisplayName = profile.DisplayName;
                friend.PictureUrl = profile.PictureUrl;
            }
        }
        catch (LineApiException)
        {
            // 封鎖的使用者查不到個人資料，正常
        }
    }
}
