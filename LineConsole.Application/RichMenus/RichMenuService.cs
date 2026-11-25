using System.Net;
using LineConsole.Application.Common.Models;
using LineConsole.Application.Exceptions;
using LineConsole.Application.Infrastructure.Interfaces;
using LineConsole.Application.LineOfficialAccounts.Interfaces;
using LineConsole.Application.RichMenus.Interfaces;
using LineConsole.Application.RichMenus.Models;
using LineConsole.Domain.Entities;

namespace LineConsole.Application.RichMenus;

/// <summary>
/// 圖文選單的用例：選單本身存在 LINE 上，本地另外記一份（點擊區塊、建立時間、排程要用的 LINE ID 對應）。
/// 每個操作都先經過 <see cref="IOfficialAccountAccess"/>：只能操作自己的官方帳號。
/// LINE 回 404 的情況一律轉成我們自己的 <see cref="NotFoundException"/>（前端拿到 404，不是 500）。
/// </summary>
public class RichMenuService : IRichMenuService
{
    private const string NotFoundCode = "RICH_MENU_NOT_FOUND";

    private readonly ILineClient _lineClient;
    private readonly IOfficialAccountAccess _accountAccess;
    private readonly IRichMenuRepository _richMenuRepo;

    public RichMenuService(
        ILineClient lineClient,
        IOfficialAccountAccess accountAccess,
        IRichMenuRepository richMenuRepo)
    {
        _lineClient = lineClient;
        _accountAccess = accountAccess;
        _richMenuRepo = richMenuRepo;
    }

    public async Task<RichMenuIdResult> CreateRichMenuWithImageAsync(
        Guid lineOfficialAccountId,
        RichMenuDTO richMenu,
        Stream imageStream,
        string contentType,
        CancellationToken ct = default)
    {
        var token = await _accountAccess.GetAccessTokenAsync(lineOfficialAccountId, ct);

        var result = await _lineClient.SendJsonAsync<RichMenuIdResult>(
            "/v2/bot/richmenu", HttpMethod.Post, token, richMenu, ct: ct);

        if (result is null || string.IsNullOrWhiteSpace(result.RichMenuId))
            throw new InvalidOperationException("建立 Rich Menu 失敗：LINE API 未回傳 richMenuId");

        // 圖片上傳失敗：把剛在 LINE 建好的選單刪掉（補償），不留下沒有圖片的選單
        try
        {
            await _lineClient.SendStreamAsync($"/v2/bot/richmenu/{result.RichMenuId}/content", imageStream, contentType, token, ct);
        }
        catch
        {
            await _lineClient.DeleteAsync($"/v2/bot/richmenu/{result.RichMenuId}", token, ct);
            throw;
        }

        var menu = RichMenu.Create(
            lineOfficialAccountId: lineOfficialAccountId,
            lineRichMenuId: result.RichMenuId,
            name: richMenu.Name,
            chatBarText: richMenu.ChatBarText,
            selected: richMenu.Selected,
            width: richMenu.Size.Width,
            height: richMenu.Size.Height);

        foreach (var area in richMenu.Areas)
        {
            menu.Areas.Add(RichMenuArea.Create(
                richMenuId: menu.Id,
                x: area.Bounds.X,
                y: area.Bounds.Y,
                width: area.Bounds.Width,
                height: area.Bounds.Height,
                actionType: area.Action.Type,
                actionData: area.Action.Data,
                actionText: area.Action.Text,
                actionUri: area.Action.Uri,
                richMenuAliasId: area.Action.RichMenuAliasId,
                dateTimeValue: area.Action.Datetime));
        }

        await _richMenuRepo.AddAsync(menu, ct);
        return result;
    }

    public async Task ValidateRichMenuAsync(Guid lineOfficialAccountId, RichMenuDTO richMenu, CancellationToken ct = default)
    {
        var token = await _accountAccess.GetAccessTokenAsync(lineOfficialAccountId, ct);
        await _lineClient.SendJsonAsync("/v2/bot/richmenu/validate", HttpMethod.Post, token, richMenu, ct: ct);
    }

    public async Task<Stream> DownloadRichMenuImageAsync(Guid lineOfficialAccountId, string richMenuId, CancellationToken ct = default)
    {
        var token = await _accountAccess.GetAccessTokenAsync(lineOfficialAccountId, ct);
        return await NotFoundAsOurs(() => _lineClient.GetStreamAsync($"/v2/bot/richmenu/{richMenuId}/content", token, ct));
    }

    public async Task<RichMenuListResult> GetRichMenuListAsync(Guid lineOfficialAccountId, CancellationToken ct = default)
    {
        var token = await _accountAccess.GetAccessTokenAsync(lineOfficialAccountId, ct);
        return await _lineClient.GetJsonAsync<RichMenuListResult>("/v2/bot/richmenu/list", token, ct: ct)
               ?? new RichMenuListResult();
    }

    public async Task<List<RichMenuWithImageResult>> GetRichMenuListWithPreviewImageAsync(Guid lineOfficialAccountId, CancellationToken ct = default)
    {
        var token = await _accountAccess.GetAccessTokenAsync(lineOfficialAccountId, ct);
        var rawList = await GetRichMenuListAsync(lineOfficialAccountId, ct);
        var result = new List<RichMenuWithImageResult>();

        foreach (var item in rawList.RichMenus)
        {
            string base64;
            try
            {
                using var stream = await _lineClient.GetStreamAsync($"/v2/bot/richmenu/{item.RichMenuId}/content", token, ct);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms, ct);
                base64 = Convert.ToBase64String(ms.ToArray());
            }
            catch (LineApiException)
            {
                base64 = string.Empty;   // 沒有圖片的選單：前端顯示「尚無選單圖片」
            }

            result.Add(new RichMenuWithImageResult
            {
                RichMenuId = item.RichMenuId,
                Name = item.Name,
                ChatBarText = item.ChatBarText,
                Selected = item.Selected,
                Width = item.Size.Width,
                Height = item.Size.Height,
                ImageBase64 = base64
            });
        }

        return result;
    }

    public async Task<RichMenuResult> GetRichMenuByIdAsync(Guid lineOfficialAccountId, string richMenuId, CancellationToken ct = default)
    {
        var token = await _accountAccess.GetAccessTokenAsync(lineOfficialAccountId, ct);
        return await NotFoundAsOurs(() => _lineClient.GetJsonAsync<RichMenuResult>($"/v2/bot/richmenu/{richMenuId}", token, ct: ct))
               ?? throw new NotFoundException(NotFoundCode, "找不到這個圖文選單");
    }

    /// <summary>刪除 LINE 上的選單，連同本地紀錄和它的排程。LINE 上已經不在的話，只清本地紀錄</summary>
    public async Task DeleteRichMenuAsync(Guid lineOfficialAccountId, string richMenuId, CancellationToken ct = default)
    {
        var token = await _accountAccess.GetAccessTokenAsync(lineOfficialAccountId, ct);
        var local = await _richMenuRepo.FindByLineIdAsync(lineOfficialAccountId, richMenuId, ct);

        try
        {
            await _lineClient.DeleteAsync($"/v2/bot/richmenu/{richMenuId}", token, ct);
        }
        catch (LineApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            if (local is null)
                throw new NotFoundException(NotFoundCode, "找不到這個圖文選單");
        }

        if (local is not null)
            await _richMenuRepo.DeleteAsync(local.Id, ct);
    }

    public async Task SetDefaultRichMenuAsync(Guid lineOfficialAccountId, string richMenuId, CancellationToken ct = default)
    {
        var token = await _accountAccess.GetAccessTokenAsync(lineOfficialAccountId, ct);
        await NotFoundAsOurs(async () =>
        {
            await _lineClient.SendJsonAsync($"/v2/bot/user/all/richmenu/{richMenuId}", HttpMethod.Post, token, data: null, ct: ct);
            return true;
        });
    }

    /// <summary>目前的預設選單；沒有設定時回傳 null（不是錯誤）</summary>
    public async Task<RichMenuIdResult?> GetDefaultRichMenuAsync(Guid lineOfficialAccountId, CancellationToken ct = default)
    {
        var token = await _accountAccess.GetAccessTokenAsync(lineOfficialAccountId, ct);
        try
        {
            var result = await _lineClient.GetJsonAsync<RichMenuIdResult>("/v2/bot/user/all/richmenu", token, ct: ct);
            return string.IsNullOrWhiteSpace(result?.RichMenuId) ? null : result;
        }
        catch (LineApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task ClearDefaultRichMenuAsync(Guid lineOfficialAccountId, CancellationToken ct = default)
    {
        var token = await _accountAccess.GetAccessTokenAsync(lineOfficialAccountId, ct);
        await _lineClient.DeleteAsync("/v2/bot/user/all/richmenu", token, ct);
    }

    private static async Task<T> NotFoundAsOurs<T>(Func<Task<T>> call)
    {
        try
        {
            return await call();
        }
        catch (LineApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new NotFoundException(NotFoundCode, "找不到這個圖文選單");
        }
    }
}
