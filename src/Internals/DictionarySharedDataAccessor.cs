using System.Collections.Generic;
using IOKode.OpinionatedFramework.Common;

namespace IOKode.OpinionatedFramework.Internals;

public class DictionarySharedDataAccessor(Dictionary<string, object?> dict) : ISharedDataAccessor
{
    public bool Exists(string key)
    {
        return dict.ContainsKey(key);
    }

    public object? Get(string key)
    {
        return dict[key];
    }

    public object? GetOrDefault(string key)
    {
        return dict.GetValueOrDefault(key);
    }

    public void Set(string key, object? value)
    {
        dict[key] = value;
    }

    public void Remove(string key)
    {
        dict.Remove(key);
    }

    public IReadOnlyDictionary<string, object?> ToReadonlyDictionary()
    {
        return dict;
    }
}