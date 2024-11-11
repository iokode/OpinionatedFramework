using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.Helpers;

public class XUnitLogger(ITestOutputHelper testOutputHelper, LoggerExternalScopeProvider scopeProvider, string categoryName) : ILogger
{
    public XUnitLogger(ITestOutputHelper testOutputHelper, LoggerExternalScopeProvider scopeProvider, Type type)
        : this(testOutputHelper, scopeProvider, type.FullName!)
    {
        
    }
    
    public static ILogger CreateLogger(ITestOutputHelper testOutputHelper, string categoryName) => new XUnitLogger(testOutputHelper, new LoggerExternalScopeProvider(), categoryName);
    public static ILogger CreateLogger(ITestOutputHelper testOutputHelper, Type categoryType) => new XUnitLogger(testOutputHelper, new LoggerExternalScopeProvider(), categoryType);

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public IDisposable BeginScope<TState>(TState state) => scopeProvider.Push(state);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        var sb = new StringBuilder();
        sb.Append("From XUnitLogger:\n")
          .Append(GetLogLevelString(logLevel))
          .Append(" [")
          .Append(categoryName)
          .Append("] ")
          .Append(formatter(state, exception));

        if (exception != null)
        {
            sb.Append('\n').Append(exception);
        }

        scopeProvider.ForEachScope((scope, state) =>
        {
            state.Append("\n => ");
            state.Append(scope);
        }, sb);

        testOutputHelper.WriteLine(sb.ToString());
    }

    private static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => nameof(LogLevel.Trace),
            LogLevel.Debug => nameof(LogLevel.Debug),
            LogLevel.Information => nameof(LogLevel.Information),
            LogLevel.Warning => nameof(LogLevel.Warning),
            LogLevel.Error => nameof(LogLevel.Error),
            LogLevel.Critical => nameof(LogLevel.Critical),
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
        };
    }
}