using System;
using Cronos;

namespace IOKode.OpinionatedFramework.Jobs;

/// <summary>
/// Represents the core details of a job that needs to be scheduled.
/// </summary>
/// <remarks>
/// This abstract class is designed to encapsulate essential aspects of a job that is to be scheduled,
/// its primary purpose is to serve as a foundational structure for managing metadata associated with
/// scheduled jobs in scheduling frameworks.
/// This class itself is not intended to represent the actual execution logic of the job, 
/// but rather its scheduling details.
/// </remarks>
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

/// <summary>
/// Extends <see cref="ScheduledJob"/> by allowing modifications to the execution interval.
/// </summary>
/// <remarks>
/// It is designed to support scenarios where the scheduling 
/// details of a job may need to change after it has been initially defined.
/// Like its base class, this class focuses on managing the metadata and scheduling information for a job, 
/// rather than its actual execution logic.
/// </remarks>
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