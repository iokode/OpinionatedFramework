using System.Collections.Generic;
using IOKode.OpinionatedFramework.Commands;

namespace IOKode.OpinionatedFramework.ContractImplementations.CommandExecutor;

internal class DictionarySharedDataAccessor : ISharedDataAccessor
{
    internal readonly Dictionary<string, object?> dict;

    public DictionarySharedDataAccessor(Dictionary<string, object?> dict)
    {
        this.dict = dict;
    }

    public bool Exists(string key)
    {
        return this.dict.ContainsKey(key);
    }

    public object? Get(string key)
    {
        return this.dict[key];
    }

    public object? GetOrDefault(string key)
    {
        return this.dict.GetValueOrDefault(key);
    }

    public void Set(string key, object? value)
    {
        this.dict[key] = value;
    }

    public void Remove(string key)
    {
        this.dict.Remove(key);
    }

    public IReadOnlyDictionary<string, object?> ToReadonlyDictionary()
    {
        return this.dict;
    }
}