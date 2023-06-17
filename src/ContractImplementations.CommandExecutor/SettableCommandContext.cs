using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using IOKode.OpinionatedFramework.Commands;

namespace IOKode.OpinionatedFramework.ContractImplementations.CommandExecutor;

internal class SettableCommandContext : CommandContext
{
    private Dictionary<string, object> _sharedData = null!;

    public void SetAsExecuted() => IsExecuted = true;

    public void SetResult(object? result)
    {
        HasResult = true;
        Result = result;
    }

    public static SettableCommandContext Create(Type commandType, IEnumerable<KeyValuePair<string, object>>? sharedData,
        CancellationToken cancellationToken)
    {
        var ctx = new SettableCommandContext
        {
            CommandType = commandType,
            CancellationToken = cancellationToken,
            _sharedData = sharedData?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, object>()
        };

        return ctx;
    }

    public override object? GetSharedData(string key)
    {
        return _sharedData.GetValueOrDefault(key);
    }
}