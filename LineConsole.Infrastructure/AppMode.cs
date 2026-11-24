using Microsoft.Extensions.Configuration;

namespace LineConsole.Infrastructure;

/// <summary>
/// 執行模式（設定 LineConsole:Mode）。
/// Demo：SQLite 記憶體資料庫＋模擬的 LINE＋展示資料，不需要任何帳號，每次啟動都還原成初始狀態。
/// Live：SQL Server＋真的 LINE Messaging API，啟動時自動套用 migration。
/// </summary>
public enum AppMode
{
    Demo,
    Live
}

/// <summary>讓其他元件（例如系統資訊 API）知道現在的模式</summary>
public sealed record RuntimeInfo(AppMode Mode);

public static class AppModeExtensions
{
    /// <summary>沒設定就是 Demo：clone 下來直接跑，不需要先準備資料庫和 LINE 帳號</summary>
    public static AppMode GetAppMode(this IConfiguration configuration) =>
        Enum.TryParse<AppMode>(configuration["LineConsole:Mode"], ignoreCase: true, out var mode) ? mode : AppMode.Demo;
}
