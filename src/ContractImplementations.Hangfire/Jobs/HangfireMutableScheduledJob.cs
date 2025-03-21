using System;
using Cronos;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Jobs;

public class HangfireMutableScheduledJob<TJob> : MutableScheduledJob<TJob> where TJob : Job
{
    public HangfireMutableScheduledJob(CronExpression interval, JobCreator<TJob>? jobArguments = null) : base(interval, jobArguments)
    {
    }

    public HangfireMutableScheduledJob(CronExpression interval, Guid id, JobCreator<TJob>? jobArguments = null) : base(interval, id, jobArguments)
    {
    }
}