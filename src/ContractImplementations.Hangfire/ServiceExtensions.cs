using System;
using Hangfire;
using IOKode.OpinionatedFramework.Jobs;
using IOKode.OpinionatedFramework.ServiceContainer;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire;

public static class ServiceExtensions
{
    public static void AddHangfireJobsImplementations(this IOpinionatedServiceCollection services)
    {
        services.AddHangfireJobEnqueuer();
        services.AddHangfireJobScheduler();
    }

    public static void AddHangfireJobEnqueuer(this IOpinionatedServiceCollection services) =>
        services.AddSingleton<IJobEnqueuer>(provider =>
        {
            EnsureHangfireConfigured(provider);
            return new HangfireJobEnqueuer();
        });

    public static void AddHangfireJobScheduler(this IOpinionatedServiceCollection services) =>
        services.AddSingleton<IJobScheduler>(provider =>
            new HangfireJobScheduler(EnsureHangfireConfigured(provider)));

    /// <summary>
    /// Resolves the storage, forcing the configuration <c>AddHangfire</c> applies lazily when it was used.
    /// </summary>
    /// <remarks>
    /// <c>AddHangfire</c> runs its configuration delegate on the first resolution of
    /// <see cref="IGlobalConfiguration"/>, and the enqueuer reaches Hangfire through its static API, which reads
    /// <see cref="JobStorage.Current"/>. Forcing the configuration here means an application that only enqueues,
    /// with no server started in this process, still observes a configured Hangfire. Both lookups are optional,
    /// because an application can add these implementations on their own after configuring Hangfire itself, in
    /// which case neither service is registered and <see cref="JobStorage.Current"/> is already set.
    /// </remarks>
    private static JobStorage EnsureHangfireConfigured(IServiceProvider provider)
    {
        provider.GetService<IGlobalConfiguration>();
        return provider.GetService<JobStorage>() ?? JobStorage.Current;
    }
}
