using LineConsole.Application.Webhooks;
using LineConsole.Server.Models.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LineConsole.Server.Controllers;

/// <summary>
/// LINE Webhook 的接收端（在 LINE Developers 設定 Webhook URL 為 https://你的網域/api/webhooks/line/{官方帳號 ID}）。
/// 不需要登入：靠 X-Line-Signature 證明是 LINE 送來的（簽章不對回 401）。
/// 收到後只做驗簽、去重、存進收件匣就回 200，處理交給背景服務（LINE 要求盡快回應，逾時會重送）。
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/webhooks/line")]
public class LineWebhookController : ControllerBase
{
    private readonly ILineWebhookService _webhook;

    public LineWebhookController(ILineWebhookService webhook)
    {
        _webhook = webhook;
    }

    [HttpPost("{lineOfficialAccountId:guid}")]
    [RequestSizeLimit(1024 * 1024)]
    public async Task<ApiResponse<WebhookIngestResult>> ReceiveAsync([FromRoute] Guid lineOfficialAccountId, CancellationToken ct)
    {
        // 簽章是對「原始位元組」算的，所以自己讀 body，不能讓模型繫結先解析成物件
        using var body = new MemoryStream();
        await Request.Body.CopyToAsync(body, ct);
        var signature = Request.Headers["X-Line-Signature"].ToString();
        return ApiResponse<WebhookIngestResult>.SuccessResponse(await _webhook.IngestAsync(lineOfficialAccountId, body.ToArray(), signature, ct));
    }
}
