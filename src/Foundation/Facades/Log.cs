using System;
using Microsoft.Extensions.Logging;

namespace IOKode.OpinionatedFramework.Facades;

public static class Log<TCategory>
{
    #region Debug

    /// <summary>
    /// Formats and writes a debug log message.
    /// </summary>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogDebug(0, exception, "Error while processing request from {Address}", address)</example>
    public static void Debug(EventId eventId, Exception? exception, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Debug, eventId, exception, message, args);
    }

    /// <summary>
    /// Formats and writes a debug log message.
    /// </summary>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogDebug(0, "Processing request from {Address}", address)</example>
    public static void Debug(EventId eventId, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Debug, eventId, message, args);
    }

    /// <summary>
    /// Formats and writes a debug log message.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogDebug(exception, "Error while processing request from {Address}", address)</example>
    public static void Debug(Exception? exception, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Debug, exception, message, args);
    }

    /// <summary>
    /// Formats and writes a debug log message.
    /// </summary>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogDebug("Processing request from {Address}", address)</example>
    public static void Debug(string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Debug, message, args);
    }

    #endregion

    #region Trace

    /// <summary>
    /// Formats and writes a trace log message.
    /// </summary>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogTrace(0, exception, "Error while processing request from {Address}", address)</example>
    public static void Trace(EventId eventId, Exception? exception, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Trace, eventId, exception, message, args);
    }

    /// <summary>
    /// Formats and writes a trace log message.
    /// </summary>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogTrace(0, "Processing request from {Address}", address)</example>
    public static void Trace(EventId eventId, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Trace, eventId, message, args);
    }

    /// <summary>
    /// Formats and writes a trace log message.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogTrace(exception, "Error while processing request from {Address}", address)</example>
    public static void Trace(Exception? exception, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Trace, exception, message, args);
    }

    /// <summary>
    /// Formats and writes a trace log message.
    /// </summary>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogTrace("Processing request from {Address}", address)</example>
    public static void Trace(string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Trace, message, args);
    }

    #endregion

    #region Info

    /// <summary>
    /// Formats and writes an informational log message.
    /// </summary>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogInformation(0, exception, "Error while processing request from {Address}", address)</example>
    public static void Info(EventId eventId, Exception? exception, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Information, eventId, exception, message, args);
    }

    /// <summary>
    /// Formats and writes an informational log message.
    /// </summary>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogInformation(0, "Processing request from {Address}", address)</example>
    public static void Info(EventId eventId, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Information, eventId, message, args);
    }

    /// <summary>
    /// Formats and writes an informational log message.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogInformation(exception, "Error while processing request from {Address}", address)</example>
    public static void Info(Exception? exception, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Information, exception, message, args);
    }

    /// <summary>
    /// Formats and writes an informational log message.
    /// </summary>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogInformation("Processing request from {Address}", address)</example>
    public static void Info(string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Information, message, args);
    }

    #endregion

    #region Warning

    /// <summary>
    /// Formats and writes a warning log message.
    /// </summary>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogWarning(0, exception, "Error while processing request from {Address}", address)</example>
    public static void Warn(EventId eventId, Exception? exception, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Warning, eventId, exception, message, args);
    }

    /// <summary>
    /// Formats and writes a warning log message.
    /// </summary>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogWarning(0, "Processing request from {Address}", address)</example>
    public static void Warn(EventId eventId, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Warning, eventId, message, args);
    }

    /// <summary>
    /// Formats and writes a warning log message.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogWarning(exception, "Error while processing request from {Address}", address)</example>
    public static void Warn(Exception? exception, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Warning, exception, message, args);
    }

    /// <summary>
    /// Formats and writes a warning log message.
    /// </summary>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogWarning("Processing request from {Address}", address)</example>
    public static void Warn(string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Warning, message, args);
    }

    #endregion

    #region Error

    /// <summary>
    /// Formats and writes an error log message.
    /// </summary>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogError(0, exception, "Error while processing request from {Address}", address)</example>
    public static void Error(EventId eventId, Exception? exception, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Error, eventId, exception, message, args);
    }

    /// <summary>
    /// Formats and writes an error log message.
    /// </summary>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogError(0, "Processing request from {Address}", address)</example>
    public static void Error(EventId eventId, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Error, eventId, message, args);
    }

    /// <summary>
    /// Formats and writes an error log message.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogError(exception, "Error while processing request from {Address}", address)</example>
    public static void Error(Exception? exception, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Error, exception, message, args);
    }

    /// <summary>
    /// Formats and writes an error log message.
    /// </summary>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogError("Processing request from {Address}", address)</example>
    public static void Error(string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Error, message, args);
    }

    #endregion

    #region Critical

    /// <summary>
    /// Formats and writes a critical log message.
    /// </summary>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogCritical(0, exception, "Error while processing request from {Address}", address)</example>
    public static void Critical(EventId eventId, Exception? exception, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Critical, eventId, exception, message, args);
    }

    /// <summary>
    /// Formats and writes a critical log message.
    /// </summary>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogCritical(0, "Processing request from {Address}", address)</example>
    public static void Critical(EventId eventId, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Critical, eventId, message, args);
    }

    /// <summary>
    /// Formats and writes a critical log message.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogCritical(exception, "Error while processing request from {Address}", address)</example>
    public static void Critical(Exception? exception, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Critical, exception, message, args);
    }

    /// <summary>
    /// Formats and writes a critical log message.
    /// </summary>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogCritical("Processing request from {Address}", address)</example>
    public static void Critical(string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Critical, message, args);
    }

    #endregion

    private static ILogger<TCategory> _getLogger()
    {
        var logger = Locator.Resolve<ILogger<TCategory>>();
        return logger;
    }
}

public static class Log
{
    #region Debug

    /// <summary>
    /// Formats and writes a debug log message.
    /// </summary>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogDebug(0, exception, "Error while processing request from {Address}", address)</example>
    public static void Debug(EventId eventId, Exception? exception, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Debug, eventId, exception, message, args);
    }

    /// <summary>
    /// Formats and writes a debug log message.
    /// </summary>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogDebug(0, "Processing request from {Address}", address)</example>
    public static void Debug(EventId eventId, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Debug, eventId, message, args);
    }

    /// <summary>
    /// Formats and writes a debug log message.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogDebug(exception, "Error while processing request from {Address}", address)</example>
    public static void Debug(Exception? exception, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Debug, exception, message, args);
    }

    /// <summary>
    /// Formats and writes a debug log message.
    /// </summary>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogDebug("Processing request from {Address}", address)</example>
    public static void Debug(string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Debug, message, args);
    }

    #endregion

    #region Trace

    /// <summary>
    /// Formats and writes a trace log message.
    /// </summary>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogTrace(0, exception, "Error while processing request from {Address}", address)</example>
    public static void Trace(EventId eventId, Exception? exception, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Trace, eventId, exception, message, args);
    }

    /// <summary>
    /// Formats and writes a trace log message.
    /// </summary>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogTrace(0, "Processing request from {Address}", address)</example>
    public static void Trace(EventId eventId, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Trace, eventId, message, args);
    }

    /// <summary>
    /// Formats and writes a trace log message.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogTrace(exception, "Error while processing request from {Address}", address)</example>
    public static void Trace(Exception? exception, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Trace, exception, message, args);
    }

    /// <summary>
    /// Formats and writes a trace log message.
    /// </summary>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogTrace("Processing request from {Address}", address)</example>
    public static void Trace(string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Trace, message, args);
    }

    #endregion

    #region Info

    /// <summary>
    /// Formats and writes an informational log message.
    /// </summary>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogInformation(0, exception, "Error while processing request from {Address}", address)</example>
    public static void Info(EventId eventId, Exception? exception, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Information, eventId, exception, message, args);
    }

    /// <summary>
    /// Formats and writes an informational log message.
    /// </summary>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogInformation(0, "Processing request from {Address}", address)</example>
    public static void Info(EventId eventId, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Information, eventId, message, args);
    }

    /// <summary>
    /// Formats and writes an informational log message.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogInformation(exception, "Error while processing request from {Address}", address)</example>
    public static void Info(Exception? exception, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Information, exception, message, args);
    }

    /// <summary>
    /// Formats and writes an informational log message.
    /// </summary>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogInformation("Processing request from {Address}", address)</example>
    public static void Info(string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Information, message, args);
    }

    #endregion

    #region Warning

    /// <summary>
    /// Formats and writes a warning log message.
    /// </summary>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogWarning(0, exception, "Error while processing request from {Address}", address)</example>
    public static void Warn(EventId eventId, Exception? exception, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Warning, eventId, exception, message, args);
    }

    /// <summary>
    /// Formats and writes a warning log message.
    /// </summary>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogWarning(0, "Processing request from {Address}", address)</example>
    public static void Warn(EventId eventId, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Warning, eventId, message, args);
    }

    /// <summary>
    /// Formats and writes a warning log message.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogWarning(exception, "Error while processing request from {Address}", address)</example>
    public static void Warn(Exception? exception, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Warning, exception, message, args);
    }

    /// <summary>
    /// Formats and writes a warning log message.
    /// </summary>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogWarning("Processing request from {Address}", address)</example>
    public static void Warn(string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Warning, message, args);
    }

    #endregion

    #region Error

    /// <summary>
    /// Formats and writes an error log message.
    /// </summary>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogError(0, exception, "Error while processing request from {Address}", address)</example>
    public static void Error(EventId eventId, Exception? exception, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Error, eventId, exception, message, args);
    }

    /// <summary>
    /// Formats and writes an error log message.
    /// </summary>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogError(0, "Processing request from {Address}", address)</example>
    public static void Error(EventId eventId, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Error, eventId, message, args);
    }

    /// <summary>
    /// Formats and writes an error log message.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogError(exception, "Error while processing request from {Address}", address)</example>
    public static void Error(Exception? exception, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Error, exception, message, args);
    }

    /// <summary>
    /// Formats and writes an error log message.
    /// </summary>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogError("Processing request from {Address}", address)</example>
    public static void Error(string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Error, message, args);
    }

    #endregion

    #region Critical

    /// <summary>
    /// Formats and writes a critical log message.
    /// </summary>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogCritical(0, exception, "Error while processing request from {Address}", address)</example>
    public static void Critical(EventId eventId, Exception? exception, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Critical, eventId, exception, message, args);
    }

    /// <summary>
    /// Formats and writes a critical log message.
    /// </summary>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogCritical(0, "Processing request from {Address}", address)</example>
    public static void Critical(EventId eventId, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Critical, eventId, message, args);
    }

    /// <summary>
    /// Formats and writes a critical log message.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogCritical(exception, "Error while processing request from {Address}", address)</example>
    public static void Critical(Exception? exception, string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Critical, exception, message, args);
    }

    /// <summary>
    /// Formats and writes a critical log message.
    /// </summary>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>_getLogger().LogCritical("Processing request from {Address}", address)</example>
    public static void Critical(string? message, params object?[] args)
    {
        _getLogger().Log(LogLevel.Critical, message, args);
    }

    #endregion

    private static ILogger _getLogger()
    {
        var logger = Locator.Resolve<ILogger>();
        return logger;
    }
}