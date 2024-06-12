using IOKode.OpinionatedFramework.ConfigureApplication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IConfigurationProvider = IOKode.OpinionatedFramework.Configuration.IConfigurationProvider;

namespace IOKode.OpinionatedFramework.ContractImplementations.MicrosoftConfiguration;

public static class ServiceExtensions
{
    public static void AddMicrosoftConfiguration(this IOpinionatedServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConfigurationProvider>(_ => new MicrosoftConfigurationProvider(configuration));
    }
}