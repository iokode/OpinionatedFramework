using IOKode.OpinionatedFramework.Foundation;
using Microsoft.Extensions.Logging;

namespace IOKode.OpinionatedFramework.Contracts.Facades;

public static class Log<TCategory>
{
    public static void Info(string message)
    {
        _getLogger().LogInformation(message);
    }

    public static void Warn(string message)
    {
        _getLogger().LogWarning(message);
    }

    public static void Critical(string message)
    {
        _getLogger().LogCritical(message);
    }

    public static void Debug(string message)
    {
        _getLogger().LogDebug(message);
    }

    public static void Trace(string message)
    {
        _getLogger().LogTrace(message);
    }

    public static void Error(string message)
    {
        _getLogger().LogError(message);
    }

    public static ILogger<TCategory> _getLogger()
    {
        var logger = Locator.Resolve<ILogger<TCategory>>();
        return logger;
    }
}