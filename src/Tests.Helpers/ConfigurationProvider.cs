using System.Collections.Concurrent;
using System.Collections.Generic;
using IOKode.OpinionatedFramework.Configuration;

namespace IOKode.OpinionatedFramework.Tests.Helpers;

public class ConfigurationProvider(IDictionary<string, object> values) : IConfigurationProvider
{
    private readonly ConcurrentDictionary<string, object> values = new(values);

    public T? GetValue<T>(string key)
    {
        var exists = values.TryGetValue(key, out var value);
        return exists ? (T) value! : default;
    }
}