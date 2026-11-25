using LineConsole.Application.Common.Models;
using LineConsole.Application.Exceptions;
using LineConsole.Application.Webhooks;
using LineConsole.Server.Models.Api;

namespace LineConsole.Server.Middlewares;

/// <summary>
/// 把例外統一轉成 ApiResponse：Webhook 簽章不對 401、找不到 404、應用層錯誤 400、LINE API 錯誤 400、其他 500
/// </summary>
public class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionMiddleware> _logger;

    public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (WebhookSignatureException ex)
        {
            _logger.LogWarning("拒絕 Webhook：{Path} 的 X-Line-Signature 驗證失敗", context.Request.Path);
            await WriteErrorAsync(context, StatusCodes.Status401Unauthorized, "WEBHOOK_SIGNATURE_INVALID", ex.Message);
        }
        catch (NotFoundException ex)
        {
            await WriteErrorAsync(context, StatusCodes.Status404NotFound, ex.Code, ex.Message);
        }
        catch (AppException ex)
        {
            await WriteErrorAsync(context, StatusCodes.Status400BadRequest, ex.Code, ex.Message);
        }
        catch (LineApiException ex)
        {
            _logger.LogWarning(ex, "LINE API 錯誤：{Message}", ex.Message);
            await WriteErrorAsync(context, StatusCodes.Status400BadRequest, "LINE_API_FAIL", ex.ParsedMessage ?? ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "未預期例外");
            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, "SERVER_ERROR", "伺服器發生錯誤");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string code, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<ApiEmptyResult>.FailResponse(code, message);
        await context.Response.WriteAsJsonAsync(response);
    }
}
