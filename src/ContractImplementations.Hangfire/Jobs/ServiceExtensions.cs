using System;
using Hangfire;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Jobs;

public static class ServiceExtensions
{
    public static void AddHangfireJobsImplementations(this IOpinionatedServiceCollection services, Action<IGlobalConfiguration> configuration)
    {
        services.AddSingleton<IJobEnqueuer, HangfireJobEnqueuer>();
        services.AddSingleton<IJobScheduler, HangfireJobScheduler>();

        configuration.Invoke(GlobalConfiguration.Configuration);
    }
}