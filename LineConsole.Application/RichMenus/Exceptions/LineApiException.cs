using System.Net;
using System.Text.Json;

namespace LineConsole.Application.Exceptions;

/// <summary>
/// 表示與 LINE API 溝通時發生的錯誤，攜帶 HTTP 狀態碼與回傳內容
/// </summary>
public class LineApiException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string ResponseContent { get; }

    /// <summary>從 LINE 回傳內容解析出的錯誤訊息（若格式合法）</summary>
    public string? ParsedMessage { get; }

    public LineApiException(string message, HttpStatusCode statusCode, string responseContent)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseContent = responseContent;
        ParsedMessage = TryExtractMessage(responseContent);
    }

    public LineApiException(string message, HttpStatusCode statusCode, string responseContent, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ResponseContent = responseContent;
        ParsedMessage = TryExtractMessage(responseContent);
    }

    private static string? TryExtractMessage(string responseContent)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseContent);
            return doc.RootElement.GetProperty("message").GetString();
        }
        catch
        {
            return null; // 非 JSON 格式或無 message 欄位
        }
    }
}
