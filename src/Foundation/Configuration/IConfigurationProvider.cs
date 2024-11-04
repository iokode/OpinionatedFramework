using IOKode.OpinionatedFramework.Facades;

namespace IOKode.OpinionatedFramework.Configuration;

[AddToFacade("Config")]
public interface IConfigurationProvider
{
    public T? GetValue<T>(string key);
}