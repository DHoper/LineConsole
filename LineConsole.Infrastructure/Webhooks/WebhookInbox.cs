using System.Threading.Channels;
using LineConsole.Application.Webhooks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LineConsole.Infrastructure.Webhooks;

/// <summary>
/// 「收件匣有新事件」的訊號：容量 1、滿了就丟掉（只需要知道「有」，不需要知道幾筆），
/// 所以一次湧進很多 Webhook 也只會叫醒背景服務一次。
/// </summary>
public sealed class WebhookInboxNotifier : IWebhookInboxNotifier
{
    private readonly Channel<bool> _signal = Channel.CreateBounded<bool>(new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropWrite });

    public void Notify() => _signal.Writer.TryWrite(true);

    internal ChannelReader<bool> Reader => _signal.Reader;
}

/// <summary>
/// 背景服務：處理 Webhook 收件匣。收到訊號馬上處理；沒有訊號也每 5 秒檢查一次（例如重開機後還有沒處理完的）。
/// </summary>
public sealed class WebhookInboxWorker : BackgroundService
{
    private static readonly TimeSpan Poll = TimeSpan.FromSeconds(5);

    private readonly WebhookInboxNotifier _notifier;
    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<WebhookInboxWorker> _logger;

    public WebhookInboxWorker(WebhookInboxNotifier notifier, IServiceScopeFactory scopes, ILogger<WebhookInboxWorker> logger)
    {
        _notifier = notifier;
        _scopes = scopes;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                int handled;
                do
                {
                    using var scope = _scopes.CreateScope();
                    handled = await scope.ServiceProvider.GetRequiredService<IWebhookEventProcessor>().ProcessPendingAsync(100, stoppingToken);
                    if (handled > 0)
                        _logger.LogInformation("Webhook：處理了 {Count} 筆事件", handled);
                }
                while (handled == 100);   // 一次最多 100 筆，還有就繼續

                using var wait = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                wait.CancelAfter(Poll);
                try
                {
                    await _notifier.Reader.ReadAsync(wait.Token);
                }
                catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
                {
                    // 沒有訊號：輪詢時間到，照樣檢查一次
                }
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "處理 Webhook 收件匣時發生錯誤");
                await Task.Delay(Poll, stoppingToken);
            }
        }
    }
}
