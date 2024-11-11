using System;
using Cronos;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.ContractImplementations.TaskRunJobs;

internal class TaskRunMutableScheduledJob : MutableScheduledJob
{
    public DateTime LastInvocation { get; set; }
    public bool IsFinalized { get; private set; }

    public TaskRunMutableScheduledJob(CronExpression interval, IJob job) : base(interval, job)
    {
        LastInvocation = DateTime.UtcNow;
    }

    public void CancelLoop()
    {
        IsFinalized = true;
    }
}