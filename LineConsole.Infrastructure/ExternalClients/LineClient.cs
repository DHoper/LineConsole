using LineConsole.Application.Exceptions;
using LineConsole.Application.Infrastructure.Interfaces;
using LineConsole.Infrastructure.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;

namespace LineConsole.Infrastructure.ExternalClients;

/// <summary>
/// �ʸ˻P LINE API ���Ҧ� HTTP �ШD�A�䴩�h�b���BJSON�B��y�B�۰ʤ�������
/// </summary>
public class LineClient : ILineClient
{
    private readonly IExternalHttpClient _http;
    private readonly ILogger<LineClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    private const string ApiDomain = "https://api.line.me";
    private const string ApiDataDomain = "https://api-data.line.me";

    public LineClient(IExternalHttpClient http, ILogger<LineClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<T?> SendJsonAsync<T>(string endpoint, HttpMethod method, string accessToken, object? data = null, bool useDataDomain = false, CancellationToken ct = default)
    {
        var url = BuildUrl(endpoint, useDataDomain);
        var headers = BuildHeaders(accessToken, "application/json");

        try
        {
            return method switch
            {
                HttpMethod m when m == HttpMethod.Get =>
                    await _http.GetAsync<T>(url, headers, ct),

                HttpMethod m when m == HttpMethod.Post =>
                    await _http.PostAsync<T>(url, data!, headers, ct),

                _ => throw new NotSupportedException($"Method {method} not supported.")
            };
        }
        catch (HttpRequestException ex) when (ex.StatusCode is not null)
        {
            _logger.LogError(ex, "LINE API �ШD���ѡG{Method} {Url}�A��ơG{Data}", method, url, JsonSerializer.Serialize(data, _jsonOptions));
            throw new LineApiException("LINE API �^�ǿ��~", ex.StatusCode!.Value, ex.Message, ex);
        }
    }

    public async Task SendJsonAsync(string endpoint, HttpMethod method, string accessToken, object? data = null, bool useDataDomain = false, CancellationToken ct = default)
    {
        var url = BuildUrl(endpoint, useDataDomain);
        var headers = BuildHeaders(accessToken, "application/json");

        try
        {
            if (method == HttpMethod.Post)
            {
                await _http.PostAsync(url, data!, headers, ct);
            }
            else if (method == HttpMethod.Delete)
            {
                await _http.DeleteAsync(url, headers, ct);
            }
            else
            {
                throw new NotSupportedException($"Method {method} not supported.");
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode is not null)
        {
            _logger.LogError(ex, "LINE API �ШD���ѡG{Method} {Url}�A��ơG{Data}", method, url, JsonSerializer.Serialize(data, _jsonOptions));
            throw new LineApiException("LINE API �^�ǿ��~", ex.StatusCode!.Value, ex.Message, ex);
        }
    }

    public async Task SendStreamAsync(string endpoint, Stream stream, string contentType, string accessToken, CancellationToken ct = default)
    {
        var url = BuildUrl(endpoint, useDataDomain: true);
        var headers = BuildHeaders(accessToken, contentType);

        try
        {
            await _http.PostStreamAsync(url, stream, contentType, headers, ct);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is not null)
        {
            _logger.LogError(ex, "LINE API �W�Ǧ�y���ѡG{Url}", url);
            throw new LineApiException("LINE API �W�ǿ��~", ex.StatusCode!.Value, ex.Message, ex);
        }
    }

    public async Task<Stream> GetStreamAsync(string endpoint, string accessToken, CancellationToken ct = default)
    {
        var url = BuildUrl(endpoint, useDataDomain: true);
        var headers = BuildHeaders(accessToken);

        try
        {
            return await _http.GetStreamAsync(url, headers, ct);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is not null)
        {
            _logger.LogError(ex, "LINE API �U����y���ѡG{Url}", url);
            throw new LineApiException("LINE API �U�����~", ex.StatusCode!.Value, ex.Message, ex);
        }
    }

    public async Task<T?> GetJsonAsync<T>(string endpoint, string accessToken, bool useDataDomain = false, CancellationToken ct = default)
    {
        var url = BuildUrl(endpoint, useDataDomain);
        var headers = BuildHeaders(accessToken);

        try
        {
            return await _http.GetAsync<T>(url, headers, ct);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is not null)
        {
            _logger.LogError(ex, "LINE API GET �ШD���ѡG{Url}", url);
            throw new LineApiException("LINE API Ū�����~", ex.StatusCode!.Value, ex.Message, ex);
        }
    }

    public async Task DeleteAsync(string endpoint, string accessToken, CancellationToken ct = default)
    {
        var url = BuildUrl(endpoint);
        var headers = BuildHeaders(accessToken);

        try
        {
            await _http.DeleteAsync(url, headers, ct);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is not null)
        {
            _logger.LogError(ex, "LINE API �R�����ѡG{Url}", url);
            throw new LineApiException("LINE API �R�����~", ex.StatusCode!.Value, ex.Message, ex);
        }
    }

    private static string BuildUrl(string endpoint, bool useDataDomain = false) =>
        (useDataDomain ? ApiDataDomain : ApiDomain).TrimEnd('/') + "/" + endpoint.TrimStart('/');

    private static Dictionary<string, string> BuildHeaders(string accessToken, string? contentType = null)
    {
        var headers = new Dictionary<string, string>
        {
            ["Authorization"] = $"Bearer {accessToken}"
        };

        if (!string.IsNullOrWhiteSpace(contentType))
            headers["Content-Type"] = contentType;

        return headers;
    }
}
