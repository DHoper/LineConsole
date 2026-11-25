using LineConsole.Application.LineOfficialAccounts.Interfaces;
using LineConsole.Application.LineOfficialAccounts.Models;
using LineConsole.Server.Models.Api;
using Microsoft.AspNetCore.Mvc;

namespace LineConsole.Server.Controllers;

/// <summary>目前登入者的 LINE 官方帳號：列表、新增（會向 LINE 驗證 token）、解除綁定</summary>
[ApiController]
[Route("api/accounts")]
public class LineOfficialAccountsController : ControllerBase
{
    private readonly IOfficialAccountService _accounts;

    public LineOfficialAccountsController(IOfficialAccountService accounts)
    {
        _accounts = accounts;
    }

    [HttpGet]
    public async Task<ApiResponse<List<OfficialAccountSummary>>> ListAsync(CancellationToken ct) =>
        ApiResponse<List<OfficialAccountSummary>>.SuccessResponse(await _accounts.ListMineAsync(ct));

    [HttpPost]
    public async Task<ApiResponse<OfficialAccountSummary>> CreateAsync([FromBody] CreateOfficialAccountInput input, CancellationToken ct) =>
        ApiResponse<OfficialAccountSummary>.SuccessResponse(await _accounts.CreateAsync(input, ct));

    /// <summary>解除綁定：只刪本地資料，LINE 上的圖文選單不會被刪</summary>
    [HttpDelete("{lineOfficialAccountId:guid}")]
    public async Task<ApiResponse<ApiEmptyResult>> DeleteAsync([FromRoute] Guid lineOfficialAccountId, CancellationToken ct)
    {
        await _accounts.DeleteAsync(lineOfficialAccountId, ct);
        return ApiResponse<ApiEmptyResult>.SuccessResponse(new ApiEmptyResult());
    }
}
