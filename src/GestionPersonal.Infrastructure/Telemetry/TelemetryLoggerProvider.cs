using GestionPersonal.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace GestionPersonal.Infrastructure.Telemetry;

public sealed class TelemetryLoggerProvider : ILoggerProvider
{
    private readonly TelemetryCollector _collector;

    public TelemetryLoggerProvider(TelemetryCollector collector)
    {
        _collector = collector;
    }

    public ILogger CreateLogger(string categoryName) => new TelemetryLogger(categoryName, _collector);

    public void Dispose()
    {
    }

    private sealed class TelemetryLogger : ILogger
    {
        private const string AppCategoryPrefix = "GestionPersonal";

        private readonly string _category;
        private readonly TelemetryCollector _collector;

        public TelemetryLogger(string category, TelemetryCollector collector)
        {
            _category = category;
            _collector = collector;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var isAppCategory = _category.StartsWith(AppCategoryPrefix, StringComparison.Ordinal);

            if (logLevel < LogLevel.Warning && !isAppCategory)
            {
                return;
            }

            var message = formatter(state, exception);

            if (exception is not null)
            {
                _collector.Report(new ReportedException(
                    exception.GetType().FullName ?? exception.GetType().Name,
                    exception.Message,
                    exception.StackTrace,
                    RequestPath: null,
                    HttpMethod: null,
                    StatusCode: null));

                return;
            }

            _collector.Emit(new AppEvent(
                "log",
                _category,
                MapSeverity(logLevel),
                message));
        }

        private static string MapSeverity(LogLevel logLevel) => logLevel switch
        {
            LogLevel.Trace or LogLevel.Debug => "Debug",
            LogLevel.Information => "Info",
            LogLevel.Warning => "Warning",
            LogLevel.Error => "Error",
            LogLevel.Critical => "Critical",
            _ => "Info"
        };
    }
}
