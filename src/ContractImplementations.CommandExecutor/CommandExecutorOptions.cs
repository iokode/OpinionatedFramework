using System;
using System.Collections.Generic;
using IOKode.OpinionatedFramework.Commands;

namespace IOKode.OpinionatedFramework.ContractImplementations.CommandExecutor;

public class CommandExecutorOptions
{
    internal List<Type> middlewareTypes = new();
    internal ICollection<KeyValuePair<string, object?>> initialSharedData = [];

    public void AddMiddleware<TMiddleware>() where TMiddleware : CommandMiddleware
    {
        this.middlewareTypes.Add(typeof(TMiddleware));
    }

    public void SetInitialSharedData(ICollection<KeyValuePair<string, object?>> initialSharedData)
    {
        this.initialSharedData = initialSharedData;
    }
}