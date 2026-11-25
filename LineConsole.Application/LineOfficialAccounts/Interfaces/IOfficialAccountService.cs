using LineConsole.Application.LineOfficialAccounts.Models;

namespace LineConsole.Application.LineOfficialAccounts.Interfaces;

/// <summary>目前登入者的官方帳號管理</summary>
public interface IOfficialAccountService
{
    Task<List<OfficialAccountSummary>> ListMineAsync(CancellationToken ct = default);

    /// <summary>新增：先向 LINE 驗證 Access Token（GET /v2/bot/info），驗證過才存，憑證加密存放</summary>
    Task<OfficialAccountSummary> CreateAsync(CreateOfficialAccountInput input, CancellationToken ct = default);

    /// <summary>解除綁定：刪掉本地的帳號、圖文選單紀錄、排程。LINE 上的圖文選單不會被刪</summary>
    Task DeleteAsync(Guid lineOfficialAccountId, CancellationToken ct = default);
}
