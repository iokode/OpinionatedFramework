using IOKode.OpinionatedFramework.ServiceContainer;
using IOKode.OpinionatedFramework.Bootstrapping.Abstractions;
using IOKode.OpinionatedFramework.Configuration;
using IOKode.OpinionatedFramework.Jobs;
using IOKode.OpinionatedFramework.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.ContractImplementations.TaskRunJobs;

public static class ServiceExtensions
{
    public static void AddTaskRunJobScheduler(this IOpinionatedServiceCollection services)
    {
        services.AddSingleton<TaskRunJobScheduler>(serviceProvider =>
        {
            var configurationProvider = serviceProvider.GetRequiredService<IConfigurationProvider>();
            var logging = serviceProvider.GetRequiredService<ILogging>();
            return new TaskRunJobScheduler(configurationProvider, logging);
        });
        services.AddSingleton<IJobScheduler>(serviceProvider =>
            serviceProvider.GetRequiredService<TaskRunJobScheduler>());
        services.AddSingleton<IStartupTask>(serviceProvider =>
            serviceProvider.GetRequiredService<TaskRunJobScheduler>());
    }

    public static void AddTaskRunJobEnqueuer(this IOpinionatedServiceCollection services)
    {
        services.AddSingleton<TaskRunJobEnqueuer>(serviceProvider =>
        {
            var configurationProvider = serviceProvider.GetRequiredService<IConfigurationProvider>();
            return new TaskRunJobEnqueuer(configurationProvider);
        });
        services.AddSingleton<IJobEnqueuer>(serviceProvider =>
            serviceProvider.GetRequiredService<TaskRunJobEnqueuer>());
        services.AddSingleton<IStartupTask>(serviceProvider =>
            serviceProvider.GetRequiredService<TaskRunJobEnqueuer>());
    }
}
