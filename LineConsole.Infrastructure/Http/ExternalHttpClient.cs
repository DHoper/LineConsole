using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace LineConsole.Infrastructure.Http;

/// <summary>
/// 外部 API 呼叫實作，統一封裝 HttpClient 用於與第三方系統溝通
/// </summary>
public class ExternalHttpClient : IExternalHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public ExternalHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<T?> GetAsync<T>(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        AddHeaders(request, headers);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessWithContentAsync(response, cancellationToken);

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions, cancellationToken);
    }

    public async Task<Stream> GetStreamAsync(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        AddHeaders(request, headers);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessWithContentAsync(response, cancellationToken);

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    public async Task<T?> PostAsync<T>(string url, object data, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8, "application/json")
        };
        AddHeaders(request, headers);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessWithContentAsync(response, cancellationToken);

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions, cancellationToken);
    }

    public async Task PostAsync(string url, object data, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8, "application/json")
        };
        AddHeaders(request, headers);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessWithContentAsync(response, cancellationToken);
    }

    public async Task PostStreamAsync(string url, Stream stream, string contentType, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StreamContent(stream)
        };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        AddHeaders(request, headers);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessWithContentAsync(response, cancellationToken);
    }

    public async Task DeleteAsync(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, url);
        AddHeaders(request, headers);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessWithContentAsync(response, cancellationToken);
    }

    private static void AddHeaders(HttpRequestMessage request, Dictionary<string, string>? headers)
    {
        if (headers == null) return;

        foreach (var (key, value) in headers)
        {
            request.Headers.TryAddWithoutValidation(key, value);
        }
    }

    private static async Task EnsureSuccessWithContentAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException(
            $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}，內容：{content}",
            null,
            response.StatusCode);
    }
}
