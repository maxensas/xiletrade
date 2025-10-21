using Microsoft.Extensions.Logging;
using System;

namespace Xiletrade.Library.Models.Application.Diagnostic;

public class TimestampedLogger<T> : ILogger<T>
{
    private readonly ILogger<T> _inner;

    public TimestampedLogger(ILogger<T> inner)
    {
        _inner = inner;
    }

    public IDisposable BeginScope<TState>(TState state) => _inner.BeginScope(state);

    public bool IsEnabled(LogLevel logLevel) => _inner.IsEnabled(logLevel);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
                            Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");

        // Ne pas modifier state, juste modifier la sortie via formatter
        _inner.Log(logLevel, eventId, state, exception, (s, e) =>
        {
            var originalMessage = formatter(s, e);
            return $"[{timestamp}] {originalMessage}";
        });
    }
}
