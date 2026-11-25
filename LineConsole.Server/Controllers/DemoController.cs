using LineConsole.Infrastructure.Demo;
using LineConsole.Server.Models.Api;
using Microsoft.AspNetCore.Mvc;

namespace LineConsole.Server.Controllers;

/// <summary>
/// 只有展示模式有：扮演 LINE 送 Webhook 事件進來（新好友、封鎖、傳訊息、重送）。
/// 正式模式沒有註冊 <see cref="DemoWebhookSimulator"/>，一律 404。
/// </summary>
[ApiController]
[Route("api/accounts/{lineOfficialAccountId:guid}/demo")]
public class DemoController : ControllerBase
{
    [HttpPost("webhook-events")]
    public async Task<ActionResult<ApiResponse<DemoWebhookResult>>> SimulateAsync([FromRoute] Guid lineOfficialAccountId,
        [FromBody] SimulateWebhookInput input, CancellationToken ct)
    {
        var simulator = HttpContext.RequestServices.GetService<DemoWebhookSimulator>();
        if (simulator is null)
            return NotFound(ApiResponse<ApiEmptyResult>.FailResponse("NOT_FOUND", "只有展示模式可以模擬 Webhook 事件"));
        return ApiResponse<DemoWebhookResult>.SuccessResponse(await simulator.SimulateAsync(lineOfficialAccountId, input.Kind, ct));
    }
}

/// <param name="Kind">follow／unfollow／message／redelivery</param>
public sealed record SimulateWebhookInput(string Kind);
