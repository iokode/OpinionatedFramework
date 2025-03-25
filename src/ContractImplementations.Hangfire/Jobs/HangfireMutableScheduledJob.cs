using System;
using Cronos;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Jobs;

public class HangfireMutableScheduledJob<TJob> : MutableScheduledJob<TJob> where TJob : Job
{
    public HangfireMutableScheduledJob(CronExpression interval, JobCreator<TJob>? creator = null) : base(interval, creator)
    {
    }

    public HangfireMutableScheduledJob(CronExpression interval, Guid id, JobCreator<TJob>? creator = null) : base(interval, id, creator)
    {
    }
}