using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.Configuration;
using IOKode.OpinionatedFramework.Jobs;
using IOKode.OpinionatedFramework.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.ContractImplementations.TaskRunJobs;

public static class ServiceExtensions
{
    public static void AddTaskRunJobScheduler(this IOpinionatedServiceCollection services)
    {
        services.AddSingleton<IJobScheduler>(serviceProvider =>
        {
            var configurationProvider = serviceProvider.GetRequiredService<IConfigurationProvider>();
            var logging = serviceProvider.GetRequiredService<ILogging>();
            return new TaskRunJobScheduler(configurationProvider, logging);
        });
    }

    public static void AddTaskRunJobEnqueuer(this IOpinionatedServiceCollection services)
    {
        services.AddSingleton<IJobEnqueuer>(serviceProvider =>
        {
            var configurationProvider = serviceProvider.GetRequiredService<IConfigurationProvider>();
            return new TaskRunJobEnqueuer(configurationProvider);
        });
    }
}