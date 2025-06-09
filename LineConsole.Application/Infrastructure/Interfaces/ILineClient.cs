using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LineConsole.Application.Infrastructure.Interfaces
{
    /// <summary>
    /// 封裝與 LINE Messaging API 溝通的最低層級通用 HTTP 請求（支援多帳號 token）
    /// </summary>
    public interface ILineClient
    {
        /// <summary>
        /// 傳送 JSON 請求並解析為指定型別結果
        /// </summary>
        /// <typeparam name="T">預期的回傳資料型別</typeparam>
        /// <param name="endpoint">LINE API 的相對路徑</param>
        /// <param name="method">HTTP 方法（POST 或 GET）</param>
        /// <param name="accessToken">當前用戶的 LINE Channel Access Token</param>
        /// <param name="data">傳送的資料物件</param>
        /// <param name="useDataDomain">是否切換至 api-data.line.me（上傳圖片需設定為 true）</param>
        /// <param name="ct">取消控制權杖</param>
        Task<T?> SendJsonAsync<T>(
            string endpoint,
            HttpMethod method,
            string accessToken,
            object? data = null,
            bool useDataDomain = false,
            CancellationToken ct = default
        );

        /// <summary>
        /// 傳送 JSON 請求，不期待回傳資料（例如 validate、set default 等）
        /// </summary>
        Task SendJsonAsync(
            string endpoint,
            HttpMethod method,
            string accessToken,
            object? data = null,
            bool useDataDomain = false,
            CancellationToken ct = default
        );

        /// <summary>
        /// 傳送串流內容（上傳圖片至 Rich Menu 用）
        /// </summary>
        Task SendStreamAsync(
            string endpoint,
            Stream stream,
            string contentType,
            string accessToken,
            CancellationToken ct = default
        );

        /// <summary>
        /// 下載圖片串流（取得 Rich Menu 圖片用）
        /// </summary>
        Task<Stream> GetStreamAsync(
            string endpoint,
            string accessToken,
            CancellationToken ct = default
        );

        /// <summary>
        /// 發出 GET 請求並解析 JSON 結果為指定型別
        /// </summary>
        /// <typeparam name="T">預期的回傳資料型別</typeparam>
        Task<T?> GetJsonAsync<T>(
            string endpoint,
            string accessToken,
            bool useDataDomain = false,
            CancellationToken ct = default
        );

        /// <summary>
        /// 發出 DELETE 請求，不期待回傳內容
        /// </summary>
        Task DeleteAsync(
            string endpoint,
            string accessToken,
            CancellationToken ct = default
        );
    }
}
