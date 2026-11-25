using System.Net;
using LineConsole.Application.Exceptions;
using LineConsole.Application.RichMenus.Models;
using LineConsole.Infrastructure.Demo.FakeLine;

namespace LineConsole.Tests.Demo;

/// <summary>模擬的 LINE 要跟真的一樣：回應格式、檢查規則、錯誤碼</summary>
public class FakeLineClientTests
{
    private readonly FakeLineClient _line = new(new FakeLineStore());
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static RichMenuDTO Menu(string chatBarText = "點我", int areaWidth = 2500) => new()
    {
        Name = "測試選單",
        ChatBarText = chatBarText,
        Size = new MenuSize { Width = 2500, Height = 1686 },
        Areas = [new MenuArea { Bounds = new MenuBounds { X = 0, Y = 0, Width = areaWidth, Height = 1686 }, Action = new MenuAction { Type = "message", Text = "hi" } }]
    };

    [Fact]
    public async Task 建立後列表看得到_而且用LINE的欄位名稱回傳()
    {
        var created = await _line.SendJsonAsync<RichMenuIdResult>("/v2/bot/richmenu", HttpMethod.Post, "token-a", Menu(), ct: Ct);
        Assert.StartsWith("richmenu-", created!.RichMenuId);

        // LINE 回的是 {"richmenus":[...]}，程式讀的是 RichMenus：要能對得上
        var list = await _line.GetJsonAsync<RichMenuListResult>("/v2/bot/richmenu/list", "token-a", ct: Ct);
        Assert.Equal(created.RichMenuId, Assert.Single(list!.RichMenus).RichMenuId);
    }

    [Fact]
    public async Task 不同Token是不同頻道_互相看不到()
    {
        await _line.SendJsonAsync<RichMenuIdResult>("/v2/bot/richmenu", HttpMethod.Post, "token-a", Menu(), ct: Ct);
        var other = await _line.GetJsonAsync<RichMenuListResult>("/v2/bot/richmenu/list", "token-b", ct: Ct);
        Assert.Empty(other!.RichMenus);
    }

    [Fact]
    public async Task 聊天列文字超過14字_跟LINE一樣回400()
    {
        var ex = await Assert.ThrowsAsync<LineApiException>(() =>
            _line.SendJsonAsync<RichMenuIdResult>("/v2/bot/richmenu", HttpMethod.Post, "t", Menu(chatBarText: new string('字', 15)), ct: Ct));
        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.Contains("chatBarText", ex.ParsedMessage);
    }

    [Fact]
    public async Task 區塊超出圖片_回400()
    {
        var ex = await Assert.ThrowsAsync<LineApiException>(() =>
            _line.SendJsonAsync("/v2/bot/richmenu/validate", HttpMethod.Post, "t", Menu(areaWidth: 2600), ct: Ct));
        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
    }

    [Fact]
    public async Task 區塊的動作不是LINE支援的類型_回400()
    {
        var menu = Menu() with { Areas = [new MenuArea { Bounds = new MenuBounds { X = 0, Y = 0, Width = 2500, Height = 1686 }, Action = new MenuAction { Type = "none" } }] };
        var ex = await Assert.ThrowsAsync<LineApiException>(() =>
            _line.SendJsonAsync<RichMenuIdResult>("/v2/bot/richmenu", HttpMethod.Post, "t", menu, ct: Ct));
        Assert.Contains("invalid action", ex.ParsedMessage);
    }

    [Fact]
    public async Task 失效的Token_LINE回401()
    {
        var ex = await Assert.ThrowsAsync<LineApiException>(() => _line.GetJsonAsync<RichMenuListResult>("/v2/bot/richmenu/list", "invalid-token", ct: Ct));
        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
    }

    [Fact]
    public async Task 刪除不存在的選單_回404()
    {
        var ex = await Assert.ThrowsAsync<LineApiException>(() => _line.DeleteAsync("/v2/bot/richmenu/richmenu-0123abcd", "t", Ct));
        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task 上傳圖片後下載得到_設定預設後查得到()
    {
        var id = (await _line.SendJsonAsync<RichMenuIdResult>("/v2/bot/richmenu", HttpMethod.Post, "t", Menu(), ct: Ct))!.RichMenuId;
        await _line.SendStreamAsync($"/v2/bot/richmenu/{id}/content", new MemoryStream([1, 2, 3]), "image/jpeg", "t", Ct);

        await using var image = await _line.GetStreamAsync($"/v2/bot/richmenu/{id}/content", "t", Ct);
        Assert.Equal(3, image.Length);

        await _line.SendJsonAsync($"/v2/bot/user/all/richmenu/{id}", HttpMethod.Post, "t", ct: Ct);
        var def = await _line.GetJsonAsync<RichMenuIdResult>("/v2/bot/user/all/richmenu", "t", ct: Ct);
        Assert.Equal(id, def!.RichMenuId);
    }
}
