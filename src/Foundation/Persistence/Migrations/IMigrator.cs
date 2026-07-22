using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Persistence.Migrations;

/// <summary>
/// Applies the application's database migrations.
/// </summary>
/// <remarks>
/// Migration execution is a capability of its own, independent of the unit-of-work and query-execution
/// capabilities. An application can run migrations without selecting any persistence driver.
/// </remarks>
public interface IMigrator
{
    /// <summary>
    /// Applies every migration that has not yet been applied.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task MigrateUpAsync(CancellationToken cancellationToken = default);
}
