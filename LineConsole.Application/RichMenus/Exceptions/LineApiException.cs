using System.Net;
using System.Text.Json;

namespace LineConsole.Application.Exceptions;

/// <summary>
/// ��ܻP LINE API ���q�ɵo�ͪ����~�A��a HTTP ���A�X�P�^�Ǥ��e
/// </summary>
public class LineApiException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string ResponseContent { get; }

    /// <summary>�q LINE �^�Ǥ��e�ѪR�X�����~�T���]�Y�榡�X�k�^</summary>
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
            return null; // �D JSON �榡�εL message ���
        }
    }
}
