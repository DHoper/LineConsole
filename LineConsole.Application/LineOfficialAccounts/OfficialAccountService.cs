using System.Net;
using LineConsole.Application.Common.Interfaces;
using LineConsole.Application.Common.Models;
using LineConsole.Application.Exceptions;
using LineConsole.Application.Infrastructure.Interfaces;
using LineConsole.Application.LineOfficialAccounts.Interfaces;
using LineConsole.Application.LineOfficialAccounts.Models;
using LineConsole.Application.UserProfiles.Interfaces;
using LineConsole.Domain.Entities;

namespace LineConsole.Application.LineOfficialAccounts;

public class OfficialAccountService : IOfficialAccountService
{
    private readonly ICurrentUser _currentUser;
    private readonly IUserProfileRepository _profiles;
    private readonly ILineOfficialAccountRepository _accounts;
    private readonly ILineClient _line;

    public OfficialAccountService(ICurrentUser currentUser, IUserProfileRepository profiles, ILineOfficialAccountRepository accounts, ILineClient line)
    {
        _currentUser = currentUser;
        _profiles = profiles;
        _accounts = accounts;
        _line = line;
    }

    public async Task<List<OfficialAccountSummary>> ListMineAsync(CancellationToken ct = default)
    {
        var accounts = await _accounts.ListByIdentityUserAsync(RequireUserId(), ct);
        return accounts.Select(ToSummary).ToList();
    }

    public async Task<OfficialAccountSummary> CreateAsync(CreateOfficialAccountInput input, CancellationToken ct = default)
    {
        var channelId = input.ChannelId.Trim();
        var secret = input.ChannelSecret.Trim();
        var token = input.ChannelAccessToken.Trim();
        if (channelId.Length == 0 || secret.Length == 0 || token.Length == 0)
            throw new AppException("ACCOUNT_INVALID", "Channel ID、Channel Secret、Access Token 都要填");

        var profile = await _profiles.FindByIdentityUserIdAsync(RequireUserId())
                      ?? throw new NotFoundException("PROFILE_NOT_FOUND", "找不到使用者資料");

        if (await _accounts.ExistsChannelAsync(profile.Id, channelId, ct))
            throw new AppException("ACCOUNT_DUPLICATED", "這個官方帳號已經綁定過了");

        var bot = await VerifyWithLineAsync(token, ct);

        var name = string.IsNullOrWhiteSpace(input.ChannelName) ? bot.DisplayName : input.ChannelName.Trim();
        var account = LineOfficialAccount.Create(profile.Id, channelId, secret, token, name);
        account.BasicId = string.IsNullOrWhiteSpace(bot.BasicId) ? null : bot.BasicId;
        await _accounts.AddAsync(account, ct);
        return ToSummary(account);
    }

    public async Task DeleteAsync(Guid lineOfficialAccountId, CancellationToken ct = default)
    {
        _ = await _accounts.FindOwnedAsync(lineOfficialAccountId, RequireUserId(), ct)
            ?? throw new NotFoundException(OfficialAccountAccess.NotFoundCode, "找不到這個 LINE 官方帳號");
        await _accounts.DeleteAsync(lineOfficialAccountId, ct);
    }

    /// <summary>用這個 token 呼叫 LINE 的 bot info：LINE 接受才代表 token 是真的、沒過期</summary>
    private async Task<LineBotInfo> VerifyWithLineAsync(string token, CancellationToken ct)
    {
        try
        {
            return await _line.GetJsonAsync<LineBotInfo>("/v2/bot/info", token, ct: ct)
                   ?? throw new AppException("LINE_VERIFY_FAILED", "LINE 沒有回傳官方帳號資訊");
        }
        catch (LineApiException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            throw new AppException("LINE_TOKEN_INVALID", "LINE 拒絕了這個 Access Token，請確認有沒有貼錯或已經過期");
        }
        catch (LineApiException ex)
        {
            throw new AppException("LINE_VERIFY_FAILED", $"向 LINE 驗證失敗：{ex.ParsedMessage ?? ex.Message}");
        }
    }

    private string RequireUserId() =>
        _currentUser.IdentityUserId ?? throw new AppException("UNAUTHENTICATED", "請先登入");

    private static OfficialAccountSummary ToSummary(LineOfficialAccount a) =>
        new(a.Id, a.ChannelName, a.ChannelId, a.BasicId, DateTime.SpecifyKind(a.CreatedAt, DateTimeKind.Utc));
}
