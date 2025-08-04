using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations.ControllerGenerators;

public interface IControllersStrategy
{
    public void Configure(IApplicationBuilder app);
    public IServiceCollection ConfigureServices(IServiceCollection services);
}