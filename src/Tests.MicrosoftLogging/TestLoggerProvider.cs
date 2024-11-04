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

public class TestLogger : ILogger
{
    private readonly string _categoryName;

    public TestLogger(string categoryName)
    {
        _categoryName = categoryName;
    }

    public List<LogEntry> LogEntries { get; } = new List<LogEntry>();

    public IDisposable BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        LogEntries.Add(new LogEntry
        {
            LogLevel = logLevel,
            EventId = eventId,
            State = state,
            Exception = exception,
            Message = formatter(state, exception),
            CategoryName = _categoryName
        });
    }
}

public class LogEntry
{
    public LogLevel LogLevel { get; set; }
    public EventId EventId { get; set; }
    public object State { get; set; }
    public Exception Exception { get; set; }
    public string Message { get; set; }
    public string CategoryName { get; set; }
}
