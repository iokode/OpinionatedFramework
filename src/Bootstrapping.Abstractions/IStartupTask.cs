using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Bootstrapping.Abstractions;

/// <summary>
/// Performs work after the framework service provider has been initialized and before the host begins normal work.
/// </summary>
/// <example>A database implementation can provide a startup task that applies pending migrations.</example>
public interface IStartupTask
{
    /// <summary>Starts the framework-owned runtime component or operation.</summary>
    Task StartAsync(CancellationToken cancellationToken = default);
}
