using System;
using System.Collections.Concurrent;
using IOKode.OpinionatedFramework.Logging;
using Microsoft.Extensions.Logging;

namespace IOKode.OpinionatedFramework.ContractImplementations.MicrosoftLogging;

public class Logging : ILogging
{
    private readonly ILoggerFactory loggerFactory;
    private ConcurrentDictionary<string, ILogger> loggers = new();

    public Logging(ILoggerFactory loggerFactory)
    {
        this.loggerFactory = loggerFactory;
    }

    public ILogger FromCategory(string category)
    {
        return loggers.GetOrAdd(category, cat => loggerFactory.CreateLogger(cat));
    }

    public ILogger FromCategory(Type categoryType)
    {
        string categoryName = TypeNameHelper.GetTypeDisplayName(categoryType);
        return FromCategory(categoryName);
    }
}