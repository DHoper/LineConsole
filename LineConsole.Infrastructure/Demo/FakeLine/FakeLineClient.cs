using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using LineConsole.Application.Exceptions;
using LineConsole.Application.Infrastructure.Interfaces;

namespace LineConsole.Infrastructure.Demo.FakeLine;

/// <summary>
/// 展示模式用的 LINE Messaging API：實作跟正式版一樣的 <see cref="ILineClient"/>，
/// 應用層（RichMenuService）完全不知道自己接的是假的。
///
/// 做法：照 LINE 的端點路由，回應先組成 LINE 格式的 JSON，再用跟正式版相同的序列化設定轉成型別，
/// 所以欄位對應（例如 LINE 回 "richmenus"、程式讀 RichMenus）也有真的走過一次。
/// 錯誤也照 LINE 的樣子丟 <see cref="LineApiException"/>（404、400 帶 message）。
/// </summary>
public sealed partial class FakeLineClient : ILineClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly FakeLineStore _store;

    public FakeLineClient(FakeLineStore store)
    {
        _store = store;
    }

    public Task<T?> SendJsonAsync<T>(string endpoint, HttpMethod method, string accessToken, object? data = null, bool useDataDomain = false, CancellationToken ct = default)
    {
        var channel = _store[accessToken];
        var path = Normalize(endpoint);

        if (method == HttpMethod.Post && path == "/v2/bot/richmenu")
        {
            var menu = ToJsonObject(data);
            Validate(menu);
            menu.Remove("startTime");   // 排程是我們自己的欄位，LINE 不會存
            menu.Remove("endTime");
            var id = channel.AddMenu(menu);
            return Task.FromResult(Convert<T>(new JsonObject { ["richMenuId"] = id }));
        }

        if (method == HttpMethod.Get)
            return GetJsonAsync<T>(endpoint, accessToken, useDataDomain, ct);

        throw NotFound(path);
    }

    public Task SendJsonAsync(string endpoint, HttpMethod method, string accessToken, object? data = null, bool useDataDomain = false, CancellationToken ct = default)
    {
        var channel = _store[accessToken];
        var path = Normalize(endpoint);

        if (method == HttpMethod.Post && path == "/v2/bot/richmenu/validate")
        {
            Validate(ToJsonObject(data));
            return Task.CompletedTask;
        }

        if (method == HttpMethod.Post && SetDefaultPath().Match(path) is { Success: true } m)
        {
            if (!channel.SetDefault(m.Groups["id"].Value)) throw NotFound(path);
            return Task.CompletedTask;
        }

        if (method == HttpMethod.Delete)
            return DeleteAsync(endpoint, accessToken, ct);

        throw NotFound(path);
    }

    public async Task SendStreamAsync(string endpoint, Stream stream, string contentType, string accessToken, CancellationToken ct = default)
    {
        var path = Normalize(endpoint);
        if (ContentPath().Match(path) is not { Success: true } m)
            throw NotFound(path);

        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, ct);
        if (!_store[accessToken].SetImage(m.Groups["id"].Value, ms.ToArray(), contentType))
            throw NotFound(path);
    }

    public Task<Stream> GetStreamAsync(string endpoint, string accessToken, CancellationToken ct = default)
    {
        var path = Normalize(endpoint);
        if (ContentPath().Match(path) is { Success: true } m && _store[accessToken].GetImage(m.Groups["id"].Value) is { } bytes)
            return Task.FromResult<Stream>(new MemoryStream(bytes, writable: false));

        throw NotFound(path);
    }

    public Task<T?> GetJsonAsync<T>(string endpoint, string accessToken, bool useDataDomain = false, CancellationToken ct = default)
    {
        var channel = _store[accessToken];
        var path = Normalize(endpoint);

        if (path == "/v2/bot/richmenu/list")
        {
            var list = new JsonArray(channel.ListMenus().Select(x => (JsonNode)x).ToArray());
            return Task.FromResult(Convert<T>(new JsonObject { ["richmenus"] = list }));
        }

        if (path == "/v2/bot/user/all/richmenu")
        {
            var id = channel.DefaultMenuId ?? throw NotFound(path, "no default richmenu");
            return Task.FromResult(Convert<T>(new JsonObject { ["richMenuId"] = id }));
        }

        if (MenuPath().Match(path) is { Success: true } m)
        {
            var menu = channel.GetMenu(m.Groups["id"].Value) ?? throw NotFound(path);
            return Task.FromResult(Convert<T>(menu));
        }

        throw NotFound(path);
    }

    public Task DeleteAsync(string endpoint, string accessToken, CancellationToken ct = default)
    {
        var channel = _store[accessToken];
        var path = Normalize(endpoint);

        if (path == "/v2/bot/user/all/richmenu")
        {
            channel.ClearDefault();
            return Task.CompletedTask;
        }

        if (MenuPath().Match(path) is { Success: true } m && channel.RemoveMenu(m.Groups["id"].Value))
            return Task.CompletedTask;

        throw NotFound(path);
    }

    /// <summary>LINE 的主要檢查：尺寸、名稱、聊天列文字長度、區塊不能超出圖片</summary>
    private static void Validate(JsonObject menu)
    {
        var width = (int?)menu["size"]?["width"] ?? 0;
        var height = (int?)menu["size"]?["height"] ?? 0;
        if (width < 800 || width > 2500 || height < 250 || (double)width / height < 1.45)
            throw BadRequest("must be specified in valid size");

        var name = (string?)menu["name"] ?? "";
        if (name.Length is 0 or > 300)
            throw BadRequest("name must be between 1 and 300 characters");

        var chatBarText = (string?)menu["chatBarText"] ?? "";
        if (chatBarText.Length is 0 or > 14)
            throw BadRequest("chatBarText must be between 1 and 14 characters");

        var areas = menu["areas"]?.AsArray() ?? [];
        if (areas.Count > 20)
            throw BadRequest("areas must not exceed 20");
        foreach (var area in areas)
        {
            var b = area?["bounds"];
            var x = (int?)b?["x"] ?? -1;
            var y = (int?)b?["y"] ?? -1;
            var w = (int?)b?["width"] ?? 0;
            var h = (int?)b?["height"] ?? 0;
            if (x < 0 || y < 0 || w <= 0 || h <= 0 || x + w > width || y + h > height)
                throw BadRequest("area bounds must be inside the rich menu");
        }
    }

    private static JsonObject ToJsonObject(object? data) =>
        JsonSerializer.SerializeToNode(data, Json) as JsonObject ?? throw BadRequest("request body is required");

    private static T? Convert<T>(JsonNode node) => node.Deserialize<T>(Json);

    private static string Normalize(string endpoint) => "/" + endpoint.Trim().TrimStart('/');

    private static LineApiException NotFound(string path, string message = "Not found") =>
        new($"LINE API 回傳錯誤（{path}）", HttpStatusCode.NotFound, JsonSerializer.Serialize(new { message }));

    private static LineApiException BadRequest(string message) =>
        new("LINE API 回傳錯誤", HttpStatusCode.BadRequest, JsonSerializer.Serialize(new { message }));

    [GeneratedRegex(@"^/v2/bot/richmenu/(?<id>richmenu-[0-9a-f]+)$")]
    private static partial Regex MenuPath();

    [GeneratedRegex(@"^/v2/bot/richmenu/(?<id>richmenu-[0-9a-f]+)/content$")]
    private static partial Regex ContentPath();

    [GeneratedRegex(@"^/v2/bot/user/all/richmenu/(?<id>richmenu-[0-9a-f]+)$")]
    private static partial Regex SetDefaultPath();
}
