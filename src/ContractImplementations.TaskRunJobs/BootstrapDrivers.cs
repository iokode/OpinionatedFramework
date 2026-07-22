using IOKode.OpinionatedFramework.ContractImplementations.TaskRunJobs;
using IOKode.OpinionatedFramework.Drivers.Abstractions;
using IOKode.OpinionatedFramework.Jobs;

[assembly: BootstrapDriver<IJobEnqueuer, TaskRunJobEnqueuerBootstrapDriver>("JobEnqueuer", "task-run", true)]
[assembly: BootstrapDriver<IJobScheduler, TaskRunJobSchedulerBootstrapDriver>("JobScheduler", "task-run", true)]

namespace IOKode.OpinionatedFramework.ContractImplementations.TaskRunJobs;

public sealed class TaskRunJobEnqueuerBootstrapDriver : IBootstrapDriverRegistrar
{
    public static BootstrapValidationResult Validate(BootstrapDriverContext context)
    {
        return BootstrapValidationResult.Success;
    }

    public static void Register(BootstrapDriverContext context)
    {
        context.Services.AddTaskRunJobEnqueuer();
    }
}

public sealed class TaskRunJobSchedulerBootstrapDriver : IBootstrapDriverRegistrar
{
    public static BootstrapValidationResult Validate(BootstrapDriverContext context)
    {
        return BootstrapValidationResult.Success;
    }

    public static void Register(BootstrapDriverContext context)
    {
        context.Services.AddTaskRunJobScheduler();
    }
}
