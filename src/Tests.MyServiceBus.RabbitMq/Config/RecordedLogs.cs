using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using IOKode.OpinionatedFramework.Logging;
using Microsoft.Extensions.Logging;

namespace IOKode.OpinionatedFramework.Tests.MyServiceBus.RabbitMq.Config;

/// <summary>What was logged, reduced to where it came from and how severe it was.</summary>
public sealed record LogEntry(string Category, LogLevel Level, Exception? Exception);

/// <summary>
/// Keeps every entry the framework logs, so a test can assert on what a component wrote.
/// </summary>
/// <remarks>
/// The entries are static for the same reason the handled events are: what logs runs in scopes of its own,
/// and the tests read it afterwards.
/// </remarks>
public sealed class RecordedLogs : ILogging
{
    private static readonly ConcurrentQueue<LogEntry> entries = new();

    /// <summary>Every entry logged so far, in the order it was logged.</summary>
    public static IReadOnlyCollection<LogEntry> Entries => entries;

    public ILogger FromCategory(string category) => new RecordingLogger(category);

    public ILogger FromCategory(Type categoryType) => new RecordingLogger(categoryType.FullName ?? categoryType.Name);

    private sealed class RecordingLogger(string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            entries.Enqueue(new LogEntry(category, logLevel, exception));
        }
    }
}
