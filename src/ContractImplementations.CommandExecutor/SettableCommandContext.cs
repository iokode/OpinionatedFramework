using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using IOKode.OpinionatedFramework.Commands;

namespace IOKode.OpinionatedFramework.ContractImplementations.CommandExecutor;

internal class SettableCommandContext : ICommandContext
{
    private Dictionary<string, object?> sharedData = null!;
    private readonly Dictionary<string, object?> pipelineData = new();

    internal Dictionary<string, object?> SharedData => this.sharedData;

    public Type CommandType { get; private init; } = null!;
    public CancellationToken CancellationToken { get; set; }
    public bool IsExecuted { get; private set; }
    public bool HasResult { get; private set; }
    public object? Result { get; private set; }

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
        var context = new SettableCommandContext
        {
            CommandType = commandType,
            CancellationToken = cancellationToken,
            sharedData = initialSharedData?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, object?>()
        };

        return context;
    }

    public bool ExistsInSharedData(string key)
    {
        bool exists = this.sharedData.ContainsKey(key);
        return exists;
    }

    public object? GetFromSharedData(string key)
    {
        object? value = this.sharedData[key];
        return value;
    }

    public object? GetFromSharedDataOrDefault(string key)
    {
        object? value = this.sharedData.GetValueOrDefault(key);
        return value;
    }

    public void SetInSharedData(string key, object? value)
    {
        this.sharedData[key] = value;
    }

    public void RemoveFromSharedData(string key)
    {
        this.sharedData.Remove(key);
    }
    
    public bool ExistsInPipelineData(string key)
    {
        return this.pipelineData.ContainsKey(key);
    }

    public object? GetFromPipelineData(string key)
    {
        return this.pipelineData[key];
    }

    public object? GetFromPipelineDataOrDefault(string key)
    {
        return this.pipelineData.GetValueOrDefault(key);
    }

    public void SetInPipelineData(string key, object? value)
    {
        this.pipelineData[key] = value;
    }

    public void RemoveFromPipelineData(string key)
    {
        this.pipelineData.Remove(key);
    }
}