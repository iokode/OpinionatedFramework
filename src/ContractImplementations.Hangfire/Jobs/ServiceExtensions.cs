using IOKode.OpinionatedFramework.Bootstrapping.Abstractions;
using IOKode.OpinionatedFramework.ServiceContainer;
using IOKode.OpinionatedFramework.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Jobs;

public static class ServiceExtensions
{
    public static void AddHangfireJobsImplementations(this IOpinionatedServiceCollection services)
    {
        services.AddHangfireJobEnqueuer();
        services.AddHangfireJobScheduler();
    }

    public static void AddHangfireJobEnqueuer(this IOpinionatedServiceCollection services) =>
        services.AddSingleton<IJobEnqueuer, HangfireJobEnqueuer>();

    public static void AddHangfireJobScheduler(this IOpinionatedServiceCollection services) =>
        services.AddSingleton<IJobScheduler, HangfireJobScheduler>();

    public static void AddHangfireWorker(this IOpinionatedServiceCollection services)
    {
        services.AddSingleton<HangfireWorker>();
        services.AddSingleton<IStartupTask>(serviceProvider =>
            serviceProvider.GetRequiredService<HangfireWorker>());
    }
}
