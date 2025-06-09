using LineConsole.Application.Infrastructure.Interfaces;
using LineConsole.Application.LineOfficialAccounts.Interfaces;
using LineConsole.Application.RichMenus.Models;
using LineConsole.Application.RichMenus.Interfaces;
using LineConsole.Domain.Entities;
using System.Net.Http;

namespace LineConsole.Application.RichMenus;

/// <summary>
/// 處理 Rich Menu 模組的應用層邏輯，串接 LINE API 並管理本地資料庫
/// </summary>
public class RichMenuService : IRichMenuService
{
    private readonly ILineClient _lineClient;
    private readonly ILineOfficialAccountRepository _accountRepo;
    private readonly IRichMenuRepository _richMenuRepo;

    public RichMenuService(
        ILineClient lineClient,
        ILineOfficialAccountRepository accountRepo,
        IRichMenuRepository richMenuRepo)
    {
        _lineClient = lineClient;
        _accountRepo = accountRepo;
        _richMenuRepo = richMenuRepo;
    }

    public async Task<RichMenuIdResult> CreateRichMenuWithImageAsync(
        Guid lineOfficialAccountId,
        RichMenuDTO richMenu,
        Stream imageStream,
        string contentType,
        CancellationToken ct = default)
    {
        var token = await _accountRepo.GetAccessTokenAsync(lineOfficialAccountId, ct);

        var result = await _lineClient.SendJsonAsync<RichMenuIdResult>(
            "/v2/bot/richmenu", HttpMethod.Post, token, richMenu, ct: ct);

        if (result is null || string.IsNullOrWhiteSpace(result.RichMenuId))
            throw new InvalidOperationException("建立 Rich Menu 失敗：LINE API 未回傳 richMenuId");

        try
        {
            await _lineClient.SendStreamAsync(
                $"/v2/bot/richmenu/{result.RichMenuId}/content",
                imageStream,
                contentType,
                token,
                ct);
        }
        catch
        {
            await _lineClient.DeleteAsync($"/v2/bot/richmenu/{result.RichMenuId}", token, ct);
            throw;
        }

        var menu = RichMenu.Create(
            lineOfficialAccountId: lineOfficialAccountId,
            name: richMenu.Name,
            chatBarText: richMenu.ChatBarText,
            selected: richMenu.Selected,
            width: richMenu.Size.Width,
            height: richMenu.Size.Height
        );

        foreach (var area in richMenu.Areas)
        {
            var entity = RichMenuArea.Create(
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
                dateTimeValue: area.Action.Datetime
            );

            menu.Areas.Add(entity);
        }

        await _richMenuRepo.AddAsync(menu, ct);

        if (richMenu.StartTime.HasValue && richMenu.EndTime.HasValue)
        {
            await _richMenuRepo.AddScheduleAsync(
                lineOfficialAccountId,
                menu.Id,
                richMenu.StartTime.Value,
                richMenu.EndTime.Value,
                ct);
        }

        return result;
    }

    public async Task ValidateRichMenuAsync(Guid lineOfficialAccountId, RichMenuDTO richMenu, CancellationToken ct = default)
    {
        var token = await _accountRepo.GetAccessTokenAsync(lineOfficialAccountId, ct);
        await _lineClient.SendJsonAsync("/v2/bot/richmenu/validate", HttpMethod.Post, token, richMenu, ct: ct);
    }

    public async Task<Stream> DownloadRichMenuImageAsync(Guid lineOfficialAccountId, string richMenuId, CancellationToken ct = default)
    {
        var token = await _accountRepo.GetAccessTokenAsync(lineOfficialAccountId, ct);
        return await _lineClient.GetStreamAsync($"/v2/bot/richmenu/{richMenuId}/content", token, ct);
    }

    public async Task<RichMenuListResult> GetRichMenuListAsync(Guid lineOfficialAccountId, CancellationToken ct = default)
    {
        var token = await _accountRepo.GetAccessTokenAsync(lineOfficialAccountId, ct);
        var result = await _lineClient.GetJsonAsync<RichMenuListResult>("/v2/bot/richmenu/list", token, ct: ct);

        if (result is null)
            throw new InvalidOperationException("LINE API 未回傳 Rich Menu 清單");

        return result;
    }

    public async Task<List<RichMenuWithImageResult>> GetRichMenuListWithPreviewImageAsync(Guid lineOfficialAccountId, CancellationToken ct = default)
    {
        var token = await _accountRepo.GetAccessTokenAsync(lineOfficialAccountId, ct);
        var rawList = await GetRichMenuListAsync(lineOfficialAccountId, ct);
        var result = new List<RichMenuWithImageResult>();

        foreach (var item in rawList.RichMenus)
        {
            string base64;

            try
            {
                using var stream = await _lineClient.GetStreamAsync(
                    $"/v2/bot/richmenu/{item.RichMenuId}/content", token, ct);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms, ct);
                base64 = Convert.ToBase64String(ms.ToArray());
            }
            catch
            {
                base64 = string.Empty;
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
        var token = await _accountRepo.GetAccessTokenAsync(lineOfficialAccountId, ct);
        var result = await _lineClient.GetJsonAsync<RichMenuResult>(
            $"/v2/bot/richmenu/{richMenuId}", token, ct: ct);

        if (result is null)
            throw new InvalidOperationException($"找不到指定的 Rich Menu：{richMenuId}");

        return result;
    }

    public async Task DeleteRichMenuAsync(Guid lineOfficialAccountId, string richMenuId, CancellationToken ct = default)
    {
        var token = await _accountRepo.GetAccessTokenAsync(lineOfficialAccountId, ct);
        await _lineClient.DeleteAsync($"/v2/bot/richmenu/{richMenuId}", token, ct);

        if (Guid.TryParse(richMenuId, out var id))
            await _richMenuRepo.DeleteAsync(id, ct);
    }

    public async Task SetDefaultRichMenuAsync(Guid lineOfficialAccountId, string richMenuId, CancellationToken ct = default)
    {
        var token = await _accountRepo.GetAccessTokenAsync(lineOfficialAccountId, ct);
        await _lineClient.SendJsonAsync(
            $"/v2/bot/user/all/richmenu/{richMenuId}", HttpMethod.Post, token, data: null, ct: ct);
    }

    public async Task<RichMenuIdResult> GetDefaultRichMenuAsync(Guid lineOfficialAccountId, CancellationToken ct = default)
    {
        var token = await _accountRepo.GetAccessTokenAsync(lineOfficialAccountId, ct);
        var result = await _lineClient.GetJsonAsync<RichMenuIdResult>(
            "/v2/bot/user/all/richmenu", token, ct: ct);

        if (result is null || string.IsNullOrWhiteSpace(result.RichMenuId))
            throw new InvalidOperationException("未設定預設 Rich Menu");

        return result;
    }

    public async Task ClearDefaultRichMenuAsync(Guid lineOfficialAccountId, CancellationToken ct = default)
    {
        var token = await _accountRepo.GetAccessTokenAsync(lineOfficialAccountId, ct);
        await _lineClient.DeleteAsync("/v2/bot/user/all/richmenu", token, ct);
    }
}
