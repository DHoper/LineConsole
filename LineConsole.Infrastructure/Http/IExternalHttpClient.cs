using System.Net.Http.Headers;

namespace LineConsole.Infrastructure.Http;

/// <summary>
/// 封裝對外部 API 的 HTTP 呼叫功能，支援 GET、POST、PUT、DELETE 等通用操作
/// </summary>
public interface IExternalHttpClient
{
    /// <summary>
    /// 發送 GET 請求，回傳指定型別結果
    /// </summary>
    Task<T?> GetAsync<T>(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 發送 POST 請求，附帶 JSON 資料
    /// </summary>
    Task<T?> PostAsync<T>(string url, object data, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 發送 POST 請求，附帶 byte stream（二進位上傳用途）
    /// </summary>
    Task PostStreamAsync(string url, Stream stream, string contentType, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 發送 GET 請求，取得原始二進位串流（例如下載圖片）
    /// </summary>
    Task<Stream> GetStreamAsync(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 發送 DELETE 請求
    /// </summary>
    Task DeleteAsync(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 發送 POST 請求但不預期任何回傳資料（例如設定成功）
    /// </summary>
    Task PostAsync(string url, object data, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
}
