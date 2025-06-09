using Serilog;
using Serilog.Events;

namespace LineConsole.Infrastructure.Logging;

public static class SerilogConfigurator
{
    public static void Configure(LoggerConfiguration configuration)
    {
        var formatter = new TimeZoneAwareTextFormatter("yyyy-MM-dd HH:mm:ss", "Asia/Taipei");

        configuration
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                formatter: formatter,
                path: "log/info/log-.txt",
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Information
            )
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(logEvent =>
                    logEvent.Properties.TryGetValue("SourceContext", out var source) &&
                    source.ToString().Contains("LineConsole.Infrastructure.Clients.LineClient")
                )
                .WriteTo.File(
                    formatter: formatter,
                    path: "log/errors/lineclient/error-.txt",
                    rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: LogEventLevel.Error
                )
            )
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(logEvent =>
                    logEvent.Level == LogEventLevel.Error &&
                    (!logEvent.Properties.TryGetValue("SourceContext", out var _) ||
                     !logEvent.Properties["SourceContext"].ToString()!.Contains("LineConsole.Infrastructure.Clients.LineClient"))
                )
                .WriteTo.File(
                    formatter: formatter,
                    path: "log/errors/system/error-.txt",
                    rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: LogEventLevel.Error
                )
            );
    }
}
