using System;
using Cronos;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Jobs;

public class HangfireMutableScheduledJob<TJob> : MutableScheduledJob<TJob> where TJob : IJob
{
    public HangfireMutableScheduledJob(CronExpression interval, JobArguments<TJob>? jobArguments = null) : base(interval, jobArguments)
    {
    }

    public HangfireMutableScheduledJob(CronExpression interval, Guid id, JobArguments<TJob>? jobArguments = null) : base(interval, id, jobArguments)
    {
    }
}