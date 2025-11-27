using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace IOKode.OpinionatedFramework.Tests.MicrosoftLogging;

public class TestLoggerProvider : ILoggerProvider
{
    private readonly List<TestLogger> _loggers = new();

    public ILogger CreateLogger(string categoryName)
    {
        var logger = new TestLogger(categoryName);
        _loggers.Add(logger);
        return logger;
    }

    public void Dispose()
    {
    }

    public List<TestLogger> GetLoggers() => _loggers;
}

public class TestLogger(string categoryName) : ILogger
{
    public List<LogEntry> LogEntries { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        LogEntries.Add(new LogEntry
        {
            LogLevel = logLevel,
            EventId = eventId,
            State = state,
            Exception = exception,
            Message = formatter(state, exception),
            CategoryName = categoryName
        });
    }
}

public class LogEntry
{
    public required LogLevel LogLevel { get; init; }
    public required EventId EventId { get; init; }
    public required object? State { get; init; }
    public required Exception? Exception { get; init; }
    public required string Message { get; init; }
    public required string CategoryName { get; init; }
}
