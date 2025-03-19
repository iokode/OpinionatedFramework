using System;
using System.Threading;

namespace IOKode.OpinionatedFramework.Jobs;

/// <summary>
/// Represents the context in which a job is executed.
/// </summary>
public interface IJobExecutionContext
{
    /// <summary>
    /// Job name used in queue lists.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Gets the type of the job that is being executed.
    /// </summary>
    public Type JobType { get; }

    /// <summary>
    /// Unique identifier of the pipeline.
    /// </summary>
    public Guid TraceID { get; }

    /// <summary>
    /// Gets a cancellation token that should be used to cancel the job execution.
    /// </summary>
    public CancellationToken CancellationToken { get; }
}