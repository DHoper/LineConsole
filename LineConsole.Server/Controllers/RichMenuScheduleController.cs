using LineConsole.Application.RichMenus;
using LineConsole.Application.RichMenus.Models;
using LineConsole.Server.Models.Api;
using Microsoft.AspNetCore.Mvc;

namespace LineConsole.Server.Controllers;

/// <summary>圖文選單排程：在指定時段把某個選單設成預設，結束後自動還原</summary>
[ApiController]
[Route("api/accounts/{lineOfficialAccountId:guid}/richmenu/schedules")]
public class RichMenuScheduleController : ControllerBase
{
    private readonly IRichMenuScheduleService _schedules;

    public RichMenuScheduleController(IRichMenuScheduleService schedules)
    {
        _schedules = schedules;
    }

    [HttpGet]
    public async Task<ApiResponse<List<RichMenuScheduleResult>>> ListAsync([FromRoute] Guid lineOfficialAccountId, CancellationToken ct) =>
        ApiResponse<List<RichMenuScheduleResult>>.SuccessResponse(await _schedules.ListAsync(lineOfficialAccountId, ct));

    [HttpPost]
    public async Task<ApiResponse<RichMenuScheduleResult>> CreateAsync(
        [FromRoute] Guid lineOfficialAccountId, [FromBody] CreateRichMenuScheduleInput input, CancellationToken ct) =>
        ApiResponse<RichMenuScheduleResult>.SuccessResponse(await _schedules.CreateAsync(lineOfficialAccountId, input, ct));

    /// <summary>取消還沒開始的排程</summary>
    [HttpDelete("{scheduleId:guid}")]
    public async Task<ApiResponse<ApiEmptyResult>> CancelAsync([FromRoute] Guid lineOfficialAccountId, [FromRoute] Guid scheduleId, CancellationToken ct)
    {
        await _schedules.CancelAsync(lineOfficialAccountId, scheduleId, ct);
        return ApiResponse<ApiEmptyResult>.SuccessResponse(new ApiEmptyResult());
    }
}
