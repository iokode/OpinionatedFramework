using IOKode.OpinionatedFramework.Jobs;
using IOKode.OpinionatedFramework.ServiceContainer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire;

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
        services.AddSingleton<IHostedService>(serviceProvider =>
            serviceProvider.GetRequiredService<HangfireWorker>());
    }
}
