using System;
using Cronos;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Jobs;

public class HangfireMutableScheduledJob : MutableScheduledJob
{
    public HangfireMutableScheduledJob(CronExpression interval, IJob job) : base(interval, job)
    {
    }

    public HangfireMutableScheduledJob(CronExpression interval, IJob job, Guid id) : base(interval, job, id)
    {
    }
}