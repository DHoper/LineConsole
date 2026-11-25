using LineConsole.Application.RichMenus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LineConsole.Infrastructure.Scheduling;

/// <summary>
/// 背景服務：每隔一段時間叫 <see cref="IRichMenuScheduleRunner"/> 處理到期的排程。
/// 只負責「定時」和「每次開新的 DI scope」，規則都在應用層和領域層。
///
/// 為什麼用 BackgroundService，不用 Hangfire／Quartz：目前是單機部署、排程數量少，內建的就夠，少一個依賴、少一組資料表。
/// 限制：多台同時跑會重複處理（之後要水平擴充時，再換成有分散式鎖的排程器）。
/// 設定：LineConsole:Scheduler:Enabled（預設 true）、LineConsole:Scheduler:IntervalSeconds（預設 30；展示模式 10，看得到切換）。
/// </summary>
public sealed class RichMenuScheduleWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly TimeProvider _time;
    private readonly ILogger<RichMenuScheduleWorker> _logger;
    private readonly TimeSpan _interval;
    private readonly bool _enabled;

    public RichMenuScheduleWorker(IServiceScopeFactory scopes, TimeProvider time, IConfiguration configuration, RuntimeInfo runtime, ILogger<RichMenuScheduleWorker> logger)
    {
        _scopes = scopes;
        _time = time;
        _logger = logger;
        _enabled = configuration.GetValue("LineConsole:Scheduler:Enabled", true);
        var defaultSeconds = runtime.Mode == AppMode.Demo ? 10 : 30;
        _interval = TimeSpan.FromSeconds(Math.Max(1, configuration.GetValue("LineConsole:Scheduler:IntervalSeconds", defaultSeconds)));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_enabled)
        {
            _logger.LogInformation("圖文選單排程的背景服務已關閉（LineConsole:Scheduler:Enabled=false）");
            return;
        }

        _logger.LogInformation("圖文選單排程的背景服務啟動，每 {Seconds} 秒檢查一次", _interval.TotalSeconds);
        using var timer = new PeriodicTimer(_interval, _time);
        do
        {
            try
            {
                using var scope = _scopes.CreateScope();
                var runner = scope.ServiceProvider.GetRequiredService<IRichMenuScheduleRunner>();
                var result = await runner.RunDueAsync(_time.GetUtcNow().UtcDateTime, stoppingToken);
                if (result.AnyChange)
                    _logger.LogInformation("排程：開始 {Activated}、結束 {Completed}、跳過 {Skipped}、失敗 {Failed}",
                        result.Activated, result.Completed, result.Skipped, result.Failed);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                // 單次失敗（例如資料庫暫時連不上）不讓背景服務停掉，下一輪再試
                _logger.LogError(ex, "處理圖文選單排程時發生錯誤");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
