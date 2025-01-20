using System;
using Cronos;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.ContractImplementations.TaskRunJobs;

internal class TaskRunMutableScheduledJob<TJob> : MutableScheduledJob<TJob> where TJob : IJob
{
    public DateTime LastInvocation { get; set; }
    public bool IsFinalized { get; private set; }

    public TaskRunMutableScheduledJob(CronExpression interval, JobArguments<TJob>? jobArguments = null) : base(interval, jobArguments)
    {
        LastInvocation = DateTime.UtcNow;
    }

    public void CancelLoop()
    {
        IsFinalized = true;
    }
}