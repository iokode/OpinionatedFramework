using System;
using IOKode.OpinionatedFramework.Logging;
using Microsoft.Extensions.Logging;

namespace IOKode.OpinionatedFramework.ContractImplementations.MicrosoftEALogging;

public class Logging(ILoggerFactory loggerFactory) : ILogging
{
    public ILogger FromCategory(string category)
    {
        return loggerFactory.CreateLogger(category);
    }

    public ILogger FromCategory(Type categoryType)
    {
        return loggerFactory.CreateLogger(categoryType);
    }
}
