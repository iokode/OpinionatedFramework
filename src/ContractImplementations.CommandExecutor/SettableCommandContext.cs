using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using IOKode.OpinionatedFramework.Commands;

namespace IOKode.OpinionatedFramework.ContractImplementations.CommandExecutor;

internal class SettableCommandContext : CommandContext
{
    private Dictionary<string, object?> sharedData = null!;

    internal Dictionary<string, object?> ShareData => this.sharedData;

    private SettableCommandContext()
    {
    }

    public void SetAsExecuted() => IsExecuted = true;

    public void SetResult(object? result)
    {
        HasResult = true;
        Result = result;
    }

    public static SettableCommandContext Create(Type commandType, IEnumerable<KeyValuePair<string, object?>>? initialSharedData,
        CancellationToken cancellationToken)
    {
        var ctx = new SettableCommandContext
        {
            CommandType = commandType,
            CancellationToken = cancellationToken,
            sharedData = initialSharedData?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, object?>()
        };

        return ctx;
    }

    public override bool ExistsInSharedData(string key)
    {
        bool exists = this.sharedData.ContainsKey(key);
        return exists;
    }

    public override object? GetFromSharedData(string key)
    {
        object? value = this.sharedData[key];
        return value;
    }

    public override object? GetFromSharedDataOrDefault(string key)
    {
        object? value = this.sharedData.GetValueOrDefault(key);
        return value;
    }

    public override void SetInSharedData(string key, object? value)
    {
        this.sharedData[key] = value;
    }

    public override void RemoveFromSharedData(string key)
    {
        this.sharedData.Remove(key);
    }
}