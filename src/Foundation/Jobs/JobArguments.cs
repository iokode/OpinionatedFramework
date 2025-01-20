namespace IOKode.OpinionatedFramework.Jobs;

/// <summary>
/// Represents the arguments required for creating a job of type <typeparamref name="TJob"/>.
/// </summary>
/// <typeparam name="TJob">The type of the job that this class provides arguments for. The type must implement <see cref="IJob"/></typeparam>
public abstract record JobArguments<TJob> where TJob : IJob
{
    /// <summary>
    /// Creates an instance of the job of type <typeparamref name="TJob"/> using the defined arguments.
    /// </summary>
    /// <returns>An instance of <see cref="IJob"/>.</returns>
    public abstract IJob CreateJob();
}