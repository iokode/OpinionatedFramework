using System;
using Cronos;

namespace IOKode.OpinionatedFramework.Jobs;

public abstract class ScheduledJob
{
    public CronExpression Interval { get; protected set; }
    
    public IJob Job { get; protected init; }
    
    public Guid Identifier { get; protected init; }
    
    protected ScheduledJob(CronExpression interval, IJob job, Guid id)
    {
        Interval = interval;
        Job = job;
        Identifier = id;
    }

    protected void SetInterval(CronExpression interval)
    {
        this.Interval = interval;
    }
}

public abstract class MutableScheduledJob : ScheduledJob
{
    public void ChangeInterval(CronExpression interval)
    {
        this.SetInterval(interval);
    }

    public MutableScheduledJob(CronExpression interval, IJob job) : base(interval, job, Guid.NewGuid())
    {
    }
    
    public MutableScheduledJob(CronExpression interval, IJob job, Guid id) : base(interval, job, id)
    {
    }
}