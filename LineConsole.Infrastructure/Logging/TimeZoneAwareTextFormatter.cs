using Serilog.Events;
using Serilog.Formatting;
using System.Globalization;
using System.Runtime.InteropServices;

namespace LineConsole.Infrastructure.Logging;
public class TimeZoneAwareTextFormatter : ITextFormatter
{
    private readonly string _timestampFormat;
    private readonly TimeZoneInfo _timeZone;

    public TimeZoneAwareTextFormatter(string timestampFormat = "yyyy-MM-dd HH:mm:ss", string? timeZoneId = null)
    {
        _timestampFormat = timestampFormat;

        var resolvedId = timeZoneId ?? GetDefaultTimeZoneId();
        try
        {
            _timeZone = TimeZoneInfo.FindSystemTimeZoneById(resolvedId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[警告] 無法使用時區 {resolvedId}，已改用 UTC。錯誤：{ex.Message}");
            _timeZone = TimeZoneInfo.Utc;
        }
    }

    public void Format(LogEvent logEvent, TextWriter output)
    {
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(logEvent.Timestamp.UtcDateTime, _timeZone);
        output.Write($"[{localTime.ToString(_timestampFormat)} {logEvent.Level}] ");
        output.Write(logEvent.RenderMessage(CultureInfo.InvariantCulture));
        if (logEvent.Exception != null)
        {
            output.Write(" ");
            output.Write(logEvent.Exception);
        }
        output.WriteLine();
    }

    private static string GetDefaultTimeZoneId()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "Taipei Standard Time"  // Windows ID
            : "Asia/Taipei";          // Linux ID (IANA)
    }
}
