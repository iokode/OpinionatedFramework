using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.ContractImplementations.TaskRunJobs;

public static class ServiceExtensions
{
    public static void AddDefaultJobScheduler(this IOpinionatedServiceCollection services)
    {
        services.AddSingleton<IJobScheduler>(_ => new TaskRunJobScheduler());
    }

    public static void AddDefaultJobEnqueuer(this IOpinionatedServiceCollection services)
    {
        services.AddSingleton<IJobEnqueuer>(_ => new TaskRunJobEnqueuer());
    }
}