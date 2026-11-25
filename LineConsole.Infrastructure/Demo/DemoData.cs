namespace LineConsole.Infrastructure.Demo;

/// <summary>
/// 展示模式的初始資料。帳密會顯示在登入頁和 README：這是公開的展示帳號，資料都是假的。
/// 圖文選單的圖片在 Demo/Images（內嵌資源），點擊區塊跟圖上的分格一致。
/// </summary>
public static class DemoData
{
    public const string Email = "demo@lineconsole.dev";
    public const string Password = "Demo1234";
    public const string DisplayName = "展示帳號";

    public sealed record Area(int X, int Y, int Width, int Height, string Type, string? Text = null, string? Uri = null);

    /// <summary>排程：啟動後幾分鐘開始、持續幾分鐘</summary>
    public sealed record Schedule(int StartAfterMinutes, int DurationMinutes);

    public sealed record Menu(string Image, string Name, string ChatBarText, int Width, int Height, bool IsDefault, Area[] Areas, Schedule? Schedule = null);

    /// <summary>
    /// 好友的模擬參數：視窗開始前已經有多少好友、每天加入幾人（平日／週末）、每天封鎖的比例、哪一天辦活動（加入暴增）。
    /// Seed 固定，所以每次啟動的數字都一樣（截圖、展示腳本才對得上）。
    /// </summary>
    public sealed record Audience(int ExistingFriends, int WeekdayFollows, int WeekendFollows, double DailyChurn, int CampaignDaysAgo, int Seed);

    public sealed record Account(string ChannelName, string ChannelId, string AccessToken, string ChannelSecret, Menu[] Menus, Audience Audience);

    private const int W = 2500, H = 1686, Half = 843;

    public static readonly Account[] Accounts =
    [
        new("LineConsole 咖啡館", "2000000001", "demo-token-cafe", "demo-secret-cafe",
        [
            new("cafe-main", "咖啡館主選單", "點我看選單", W, H, IsDefault: true,
            [
                new(0, 0, 833, Half, "message", Text: "菜單"),
                new(833, 0, 833, Half, "message", Text: "集點卡"),
                new(1666, 0, 834, Half, "uri", Uri: "https://example.com/booking"),
                new(0, Half, 833, Half, "message", Text: "本週優惠"),
                new(833, Half, 833, Half, "uri", Uri: "https://example.com/store"),
                new(1666, Half, 834, Half, "message", Text: "聯絡客服"),
            ]),
            // 啟動 2 分鐘後自動換成週年慶選單，10 分鐘後還原成主選單：展示時開著畫面就看得到排程生效
            new("cafe-anniversary", "週年慶活動選單", "週年慶開跑", W, Half, IsDefault: false,
            [
                new(0, 0, 1250, Half, "message", Text: "週年慶"),
                new(1250, 0, 1250, Half, "message", Text: "抽獎活動"),
            ], Schedule: new(StartAfterMinutes: 2, DurationMinutes: 10)),
            new("cafe-member", "會員專區選單", "會員專區", W, H, IsDefault: false,
            [
                new(0, 0, 1250, H, "uri", Uri: "https://example.com/member"),
                new(1250, 0, 1250, Half, "message", Text: "我的優惠券"),
                new(1250, Half, 1250, Half, "message", Text: "邀請好友"),
            ]),
        ], new Audience(ExistingFriends: 120, WeekdayFollows: 4, WeekendFollows: 9, DailyChurn: 0.012, CampaignDaysAgo: 9, Seed: 11)),
        new("LineConsole 選物店", "2000000002", "demo-token-shop", "demo-secret-shop",
        [
            new("shop-main", "選物店主選單", "逛逛選物店", W, H, IsDefault: true,
            [
                new(0, 0, 1250, Half, "uri", Uri: "https://example.com/new"),
                new(1250, 0, 1250, Half, "uri", Uri: "https://example.com/top"),
                new(0, Half, 1250, Half, "message", Text: "訂單查詢"),
                new(1250, Half, 1250, Half, "message", Text: "客服中心"),
            ]),
            new("shop-sale", "限時特賣選單", "限時特賣中", W, Half, IsDefault: false,
            [
                new(0, 0, 833, Half, "uri", Uri: "https://example.com/sale"),
                new(833, 0, 833, Half, "message", Text: "免運門檻"),
                new(1666, 0, 834, Half, "message", Text: "滿額贈"),
            ], Schedule: new(StartAfterMinutes: 7 * 24 * 60, DurationMinutes: 3 * 24 * 60)),
        ], new Audience(ExistingFriends: 45, WeekdayFollows: 2, WeekendFollows: 4, DailyChurn: 0.02, CampaignDaysAgo: 20, Seed: 23)),
    ];
}
