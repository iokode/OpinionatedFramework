using System;
using Microsoft.Extensions.Logging;

namespace IOKode.OpinionatedFramework.Logging;

public partial interface ILogging
{
     /// <summary>
    /// Formats and writes a critical log message.
    /// </summary>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public void Critical(EventId eventId, Exception? exception, string? message, params object?[] args)
    {
        FromCaller().Log(LogLevel.Critical, eventId, exception, message, args);
    }

    /// <summary>
    /// Formats and writes a critical log message.
    /// </summary>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public void Critical(EventId eventId, string? message, params object?[] args)
    {
        FromCaller().Log(LogLevel.Critical, eventId, message, args);
    }

    /// <summary>
    /// Formats and writes a critical log message.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public void Critical(Exception? exception, string? message, params object?[] args)
    {
        FromCaller().Log(LogLevel.Critical, exception, message, args);
    }

    /// <summary>
    /// Formats and writes a critical log message.
    /// </summary>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public void Critical(string? message, params object?[] args)
    {
        FromCaller().Log(LogLevel.Critical, message, args);
    }
}