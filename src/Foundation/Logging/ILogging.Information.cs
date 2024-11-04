using System;
using Microsoft.Extensions.Logging;

namespace IOKode.OpinionatedFramework.Logging;

public partial interface ILogging
{
    /// <summary>
    /// Formats and writes an informational log message.
    /// </summary>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public void Info(EventId eventId, Exception? exception, string? message, params object?[] args)
    {
        FromCaller().Log(LogLevel.Information, eventId, exception, message, args);
    }

    /// <summary>
    /// Formats and writes an informational log message.
    /// </summary>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public void Info(EventId eventId, string? message, params object?[] args)
    {
        FromCaller().Log(LogLevel.Information, eventId, message, args);
    }

    /// <summary>
    /// Formats and writes an informational log message.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public void Info(Exception? exception, string? message, params object?[] args)
    {
        FromCaller().Log(LogLevel.Information, exception, message, args);
    }

    /// <summary>
    /// Formats and writes an informational log message.
    /// </summary>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public void Info(string? message, params object?[] args)
    {
        FromCaller().Log(LogLevel.Information, message, args);
    }
}