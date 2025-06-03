using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations.CommandControllers;

public interface IControllersStrategy
{
    public void Configure(IApplicationBuilder app);
    public IServiceCollection ConfigureServices(IServiceCollection services);
}