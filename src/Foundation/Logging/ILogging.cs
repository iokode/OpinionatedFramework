using System;
using IOKode.OpinionatedFramework.Facades;
using Microsoft.Extensions.Logging;

namespace IOKode.OpinionatedFramework.Logging;

[AddToFacade("Log")]
public partial interface ILogging
{
    public ILogger FromCategory(string category);

    public ILogger FromCategory(Type categoryType);

    public ILogger FromCategory<TCategory>()
    {
        var categoryType = typeof(TCategory);
        var logger = FromCategory(categoryType);

        return logger;
    }
}