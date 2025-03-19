namespace IOKode.OpinionatedFramework.Jobs;

/// <summary>
/// While a job are intended to be delayed and executed in background, it needs to be instanced when executed.
/// This class contains the logic to instantiate the job.
/// </summary>
/// <remarks>
/// This class usually encapsulates the arguments required for creating a job of type <typeparamref name="TJob"/>.
/// </remarks>
/// <typeparam name="TJob">The type of the job to create.</typeparam>
public abstract class JobCreator<TJob> where TJob : Job
{
    /// <summary>
    /// Creates an instance of the job of type <typeparamref name="TJob"/>.
    /// </summary>
    /// <returns>An instance of <see cref="Job"/>.</returns>
    public abstract TJob CreateJob();

    /// <summary>
    /// The job name is used by a job executor to show it in a queue.
    /// </summary>
    /// <returns>The job name.</returns>
    public abstract string GetJobName();
}