using Microsoft.Extensions.Configuration;
using IConfigurationProvider = IOKode.OpinionatedFramework.Configuration.IConfigurationProvider;

namespace IOKode.OpinionatedFramework.Tests.Helpers;

/// <typeparam name="TFromAssembly">The type from the assembly to search for an instance of <code>UserSecretsIdAttribute</code>.</typeparam>
public class ConfigurationProviderFromSecrets<TFromAssembly> : IConfigurationProvider
    where TFromAssembly : class
{
    private readonly IConfigurationRoot configuration;

    public ConfigurationProviderFromSecrets()
    {
        var builder = new ConfigurationBuilder().AddUserSecrets<TFromAssembly>();
        configuration = builder.Build();
    }

    public T? GetValue<T>(string key)
    {
        return configuration.GetValue<T>(key);
    }
}