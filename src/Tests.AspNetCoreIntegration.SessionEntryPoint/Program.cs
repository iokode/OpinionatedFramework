using IOKode.OpinionatedFramework.AspNetCoreIntegration.Session;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.Commands.Extensions;
using IOKode.OpinionatedFramework.ContractImplementations.CommandExecutor;
using IOKode.OpinionatedFramework.ContractImplementations.MicrosoftLogging;

namespace Tests.AspNetCoreIntegration.SessionEntryPoint;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var services = builder.Services;
        services.AddHttpContextAccessor();
        services.AddDistributedMemoryCache();
        services.AddSession(options => { options.Cookie.Name = ".Session"; });

        var app = builder.Build();

        var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
        Container.Services.AddSingleton(httpContextAccessor);

        Container.Services.AddMicrosoftLogging(options =>
        {
            options.AddConsole();
        });
        Container.Services.AddDefaultCommandExecutor(
            new PreSessionMiddleware(),
            new PostSessionMiddleware()
        );
        Container.Initialize();

        app.UseSession();
        app.UseRouting();

        app.MapGet("/clear-session", context =>
        {
            context.Session.Clear();
            return Task.CompletedTask;
        });

        app.MapGet("/endpoint1", async context =>
        {
            await new Command1().InvokeAsync();
            await context.Response.WriteAsync("Command1 Executed");
        });

        app.MapGet("/endpoint2", async context =>
        {
            string value = await new Command2().InvokeAsync();
            await context.Response.WriteAsync(value);
        });

        app.Run();
    }
}