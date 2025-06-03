using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations.CommandControllers.SingleController;

public record SingleControllerStrategy : IControllersStrategy 
{
    public string Route { get; init; } = "api";
    public string HttpMethod { get; init; } = "post";

    public void Configure(IApplicationBuilder app)
    {
        var cleanRoute = Route.TrimStart('/');
        
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute
            (
                name: "CommandRoute",
                pattern: $"/{cleanRoute}",
                defaults: new
                {
                    controller = "SingleCommand",
                    action = "InvokeCommand"
                }
            );
        });
    }

    public IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(this);
        services.AddControllers()
            .AddApplicationPart(typeof(SingleCommandController).Assembly);
        return services;
    }
}