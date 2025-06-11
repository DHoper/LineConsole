using LineConsole.Application.RichMenus.Models;
using LineConsole.Server.Models.Api;
using LineConsole.Application.RichMenus.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LineConsole.Server.Controllers;

/// 提供 LINE Rich Menu 模組的所有對外 Web API 入口（支援多帳號）
[ApiController]
[Route("api/accounts/{lineOfficialAccountId:guid}/richmenu")]
public class RichMenuController : ControllerBase
{
    private readonly IRichMenuService _service;

    public RichMenuController(IRichMenuService service)
    {
        _service = service;
    }

    /// 建立圖文選單並上傳圖片（multipart/form-data）
    [HttpPost("with-image")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<RichMenuIdResult>>> CreateWithImageAsync(
        [FromRoute] Guid lineOfficialAccountId,
        [FromForm] CreateRichMenuInput request,
        CancellationToken ct)
    {
        var richMenuDto = new RichMenuDTO
        {
            Selected = request.Selected,
            Name = request.Name,
            ChatBarText = request.ChatBarText,
            Size = request.Size,
            Areas = request.Areas,
            StartTime = request.ScheduleStart,
            EndTime = request.ScheduleEnd
        };

        await using var stream = request.Image.OpenReadStream();

        var result = await _service.CreateRichMenuWithImageAsync(
            lineOfficialAccountId,
            richMenuDto,
            stream,
            request.Image.ContentType,
            ct);

        return ApiResponse<RichMenuIdResult>.SuccessResponse(result);
    }

    /// 驗證圖文選單內容是否合法
    [HttpPost("validate")]
    public async Task<ActionResult<ApiResponse<ApiEmptyResult>>> ValidateAsync(
        [FromRoute] Guid lineOfficialAccountId,
        [FromBody] RichMenuDTO dto,
        CancellationToken ct)
    {
        await _service.ValidateRichMenuAsync(lineOfficialAccountId, dto, ct);
        return ApiResponse<ApiEmptyResult>.SuccessResponse(new ApiEmptyResult());
    }

    /// 下載圖文選單圖片（image/jpeg）
    [HttpGet("{richMenuId}/content")]
    public async Task<IActionResult> DownloadImageAsync(
        [FromRoute] Guid lineOfficialAccountId,
        [FromRoute] string richMenuId,
        CancellationToken ct)
    {
        var stream = await _service.DownloadRichMenuImageAsync(lineOfficialAccountId, richMenuId, ct);
        return File(stream, "image/jpeg");
    }

    /// 取得所有圖文選單列表（不含圖片）
    [HttpGet("list")]
    public async Task<ActionResult<ApiResponse<RichMenuListResult>>> GetListAsync(
        [FromRoute] Guid lineOfficialAccountId,
        CancellationToken ct)
    {
        var result = await _service.GetRichMenuListAsync(lineOfficialAccountId, ct);
        return ApiResponse<RichMenuListResult>.SuccessResponse(result);
    }

    /// 取得所有圖文選單及其預覽圖
    [HttpGet("list-with-preview")]
    public async Task<ActionResult<ApiResponse<List<RichMenuWithImageResult>>>> GetListWithPreviewAsync(
        [FromRoute] Guid lineOfficialAccountId,
        CancellationToken ct)
    {
        var result = await _service.GetRichMenuListWithPreviewImageAsync(lineOfficialAccountId, ct);
        return ApiResponse<List<RichMenuWithImageResult>>.SuccessResponse(result);
    }

    /// 取得指定 ID 的圖文選單資訊
    [HttpGet("{richMenuId}")]
    public async Task<ActionResult<ApiResponse<RichMenuResult>>> GetByIdAsync(
        [FromRoute] Guid lineOfficialAccountId,
        [FromRoute] string richMenuId,
        CancellationToken ct)
    {
        var result = await _service.GetRichMenuByIdAsync(lineOfficialAccountId, richMenuId, ct);
        return ApiResponse<RichMenuResult>.SuccessResponse(result);
    }

    /// 刪除指定圖文選單
    [HttpDelete("{richMenuId}")]
    public async Task<ActionResult<ApiResponse<ApiEmptyResult>>> DeleteAsync(
        [FromRoute] Guid lineOfficialAccountId,
        [FromRoute] string richMenuId,
        CancellationToken ct)
    {
        await _service.DeleteRichMenuAsync(lineOfficialAccountId, richMenuId, ct);
        return ApiResponse<ApiEmptyResult>.SuccessResponse(new ApiEmptyResult());
    }

    /// 設定指定圖文選單為預設選單
    [HttpPost("default/{richMenuId}")]
    public async Task<ActionResult<ApiResponse<ApiEmptyResult>>> SetDefaultAsync(
        [FromRoute] Guid lineOfficialAccountId,
        [FromRoute] string richMenuId,
        CancellationToken ct)
    {
        await _service.SetDefaultRichMenuAsync(lineOfficialAccountId, richMenuId, ct);
        return ApiResponse<ApiEmptyResult>.SuccessResponse(new ApiEmptyResult());
    }

    /// 取得目前設定為預設的圖文選單 ID
    [HttpGet("default")]
    public async Task<ActionResult<ApiResponse<RichMenuIdResult>>> GetDefaultAsync(
        [FromRoute] Guid lineOfficialAccountId,
        CancellationToken ct)
    {
        var result = await _service.GetDefaultRichMenuAsync(lineOfficialAccountId, ct);
        return ApiResponse<RichMenuIdResult>.SuccessResponse(result);
    }

    /// 清除預設圖文選單設定
    [HttpDelete("default")]
    public async Task<ActionResult<ApiResponse<ApiEmptyResult>>> ClearDefaultAsync(
        [FromRoute] Guid lineOfficialAccountId,
        CancellationToken ct)
    {
        await _service.ClearDefaultRichMenuAsync(lineOfficialAccountId, ct);
        return ApiResponse<ApiEmptyResult>.SuccessResponse(new ApiEmptyResult());
    }
}
