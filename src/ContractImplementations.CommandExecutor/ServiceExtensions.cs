using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.ConfigureApplication;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.ContractImplementations.CommandExecutor;

public static class ServiceExtensions
{
    public static void AddDefaultCommandExecutor(this IOpinionatedServiceCollection services, params CommandMiddleware[] middlewares)
    {
        services.AddSingleton<ICommandExecutor>(_ => new CommandExecutor(middlewares));
    }
}