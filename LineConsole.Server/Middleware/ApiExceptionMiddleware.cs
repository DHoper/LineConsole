using LineConsole.Application.Exceptions;
using LineConsole.Server.Models.Api;

namespace LineConsole.Server.Middlewares;
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
        catch (LineApiException ex)
        {
            _logger.LogWarning(ex, "LINE API 錯誤：{Message}", ex.Message);
            await WriteErrorAsync(context, 400, "LINE_API_FAIL", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "未預期例外");
            await WriteErrorAsync(context, 500, "SERVER_ERROR", "伺服器發生錯誤");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string code, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<ApiEmptyResult>.Fail(code, message);
        await context.Response.WriteAsJsonAsync(response);
    }
}