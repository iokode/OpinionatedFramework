using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using IOKode.OpinionatedFramework.Logging;
using Microsoft.Extensions.Logging;

namespace IOKode.OpinionatedFramework.Tests.Helpers;

public class TestsLogging : ILogging
{
    private ConcurrentDictionary<string, ILogger> loggers = new();

    public ILogger FromCategory(string category)
    {
        return loggers.GetOrAdd(category, new TestLogger(category));
    }

    public ILogger FromCategory(Type categoryType)
    {
        string categoryName = categoryType.AssemblyQualifiedName!;
        return FromCategory(categoryName);
    }
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