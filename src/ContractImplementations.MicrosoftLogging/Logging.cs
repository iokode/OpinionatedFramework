using System;
using System.Collections.Concurrent;
using IOKode.OpinionatedFramework.Logging;
using Microsoft.Extensions.Logging;

namespace IOKode.OpinionatedFramework.ContractImplementations.MicrosoftLogging;

public class Logging(ILoggerFactory loggerFactory) : ILogging
{
    private ConcurrentDictionary<string, ILogger> loggers = new();

    public ILogger FromCategory(string category)
    {
        return loggers.GetOrAdd(category, loggerFactory.CreateLogger);
    }

    public ILogger FromCategory(Type categoryType)
    {
        string categoryName = TypeNameHelper.GetTypeDisplayName(categoryType);
        return FromCategory(categoryName);
    }
}