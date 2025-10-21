using Microsoft.Extensions.Logging;
using System;

namespace Xiletrade.Library.Models.Application.Diagnostic;

public class TimestampedLoggerFactory<T> : ILogger<T>
{
    private readonly TimestampedLogger<T> _logger;

    public TimestampedLoggerFactory(ILoggerFactory factory)
    {
        var innerLogger = factory.CreateLogger<T>();
        _logger = new TimestampedLogger<T>(innerLogger);
    }

    public IDisposable BeginScope<TState>(TState state) => _logger.BeginScope(state);

    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
        Func<TState, Exception, string> formatter)
    {
        _logger.Log(logLevel, eventId, state, exception, formatter);
    }
}
