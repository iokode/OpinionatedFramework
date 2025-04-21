using System;
using Cronos;
using IOKode.OpinionatedFramework.Jobs;
using NodaTime;

namespace IOKode.OpinionatedFramework.ContractImplementations.TaskRunJobs;

internal class TaskRunMutableScheduledJob<TJob> : MutableScheduledJob<TJob> where TJob : Job
{
    public Instant LastInvocation { get; set; }
    public bool IsFinalized { get; private set; }

    public TaskRunMutableScheduledJob(CronExpression interval, JobCreator<TJob>? creator = null) : base(interval, creator)
    {
        LastInvocation = Instant.FromDateTimeUtc(DateTime.UtcNow);
    }

    public void CancelLoop()
    {
        IsFinalized = true;
    }
}