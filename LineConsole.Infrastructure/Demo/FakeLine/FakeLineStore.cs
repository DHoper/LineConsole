using System.Collections.Concurrent;
using System.Text.Json.Nodes;

namespace LineConsole.Infrastructure.Demo.FakeLine;

/// <summary>
/// 展示模式的「假 LINE 平台」狀態：每個 Channel Access Token 對應一個頻道，
/// 存它的圖文選單、圖片和預設選單。整個程式共用一份（singleton），重開程式就清空。
/// </summary>
public sealed class FakeLineStore
{
    private readonly ConcurrentDictionary<string, Channel> _channels = new();

    /// <summary>沒看過的 token 就開一個新頻道（展示模式註冊新帳號時，隨便填的 token 也能用）</summary>
    public Channel this[string accessToken] => _channels.GetOrAdd(accessToken, _ => new Channel());

    public sealed class Channel
    {
        private readonly object _gate = new();
        private readonly Dictionary<string, JsonObject> _menus = new();
        private readonly Dictionary<string, (byte[] Bytes, string ContentType)> _images = new();
        private string? _defaultMenuId;

        public string AddMenu(JsonObject menu)
        {
            var id = "richmenu-" + Guid.NewGuid().ToString("N");
            menu["richMenuId"] = id;
            lock (_gate) _menus[id] = menu;
            return id;
        }

        public JsonObject? GetMenu(string id)
        {
            lock (_gate) return _menus.TryGetValue(id, out var m) ? (JsonObject)m.DeepClone() : null;
        }

        public List<JsonObject> ListMenus()
        {
            lock (_gate) return _menus.Values.Select(m => (JsonObject)m.DeepClone()).ToList();
        }

        public bool RemoveMenu(string id)
        {
            lock (_gate)
            {
                if (!_menus.Remove(id)) return false;
                _images.Remove(id);
                if (_defaultMenuId == id) _defaultMenuId = null;
                return true;
            }
        }

        public bool SetImage(string id, byte[] bytes, string contentType)
        {
            lock (_gate)
            {
                if (!_menus.ContainsKey(id)) return false;
                _images[id] = (bytes, contentType);
                return true;
            }
        }

        public byte[]? GetImage(string id)
        {
            lock (_gate) return _images.TryGetValue(id, out var img) ? img.Bytes : null;
        }

        public bool SetDefault(string id)
        {
            lock (_gate)
            {
                if (!_menus.ContainsKey(id)) return false;
                _defaultMenuId = id;
                return true;
            }
        }

        public string? DefaultMenuId
        {
            get { lock (_gate) return _defaultMenuId; }
        }

        public void ClearDefault()
        {
            lock (_gate) _defaultMenuId = null;
        }
    }
}
