using LineConsole.Application.RichMenus.Models;

namespace LineConsole.Application.RichMenus.Interfaces;

/// <summary>
/// 定義 LINE Rich Menu 模組的所有業務操作介面，支援多帳號與完整功能調用
/// </summary>
public interface IRichMenuService
{
    Task<RichMenuIdResult> CreateRichMenuWithImageAsync(
        Guid lineOfficialAccountId,
        RichMenuDTO richMenu,
        Stream imageStream,
        string contentType,
        CancellationToken ct = default);

    Task ValidateRichMenuAsync(
        Guid lineOfficialAccountId,
        RichMenuDTO richMenu,
        CancellationToken ct = default);

    Task<Stream> DownloadRichMenuImageAsync(
        Guid lineOfficialAccountId,
        string richMenuId,
        CancellationToken ct = default);

    Task<RichMenuListResult> GetRichMenuListAsync(
        Guid lineOfficialAccountId,
        CancellationToken ct = default);

    Task<List<RichMenuWithImageResult>> GetRichMenuListWithPreviewImageAsync(
        Guid lineOfficialAccountId,
        CancellationToken ct = default);

    Task<RichMenuResult> GetRichMenuByIdAsync(
        Guid lineOfficialAccountId,
        string richMenuId,
        CancellationToken ct = default);

    Task DeleteRichMenuAsync(
        Guid lineOfficialAccountId,
        string richMenuId,
        CancellationToken ct = default);

    Task SetDefaultRichMenuAsync(
        Guid lineOfficialAccountId,
        string richMenuId,
        CancellationToken ct = default);

    Task<RichMenuIdResult> GetDefaultRichMenuAsync(
        Guid lineOfficialAccountId,
        CancellationToken ct = default);

    Task ClearDefaultRichMenuAsync(
        Guid lineOfficialAccountId,
        CancellationToken ct = default);
}
