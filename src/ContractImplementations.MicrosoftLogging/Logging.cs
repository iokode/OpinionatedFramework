using System;
using System.Collections.Generic;
using IOKode.OpinionatedFramework.Logging;
using Microsoft.Extensions.Logging;

namespace IOKode.OpinionatedFramework.ContractImplementations.MicrosoftLogging;

public class Logging : ILogging
{
    private readonly ILoggerFactory loggerFactory;
    private Dictionary<string, ILogger> loggers = new();

    public Logging(ILoggerFactory loggerFactory)
    {
        this.loggerFactory = loggerFactory;
    }

    public ILogger FromCategory(string category)
    {
        if (this.loggers.TryGetValue(category, out var logger))
        {
            return logger;
        }

        logger = this.loggerFactory.CreateLogger(category);
        this.loggers.Add(category, logger);
        return logger;
    }

    public ILogger FromCategory(Type categoryType)
    {
        string categoryName = TypeNameHelper.GetTypeDisplayName(categoryType);
        return FromCategory(categoryName);
    }
}