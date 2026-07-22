using System.Threading;
using System.Threading.Tasks;
using FluentMigrator.Runner;
using IOKode.OpinionatedFramework.Persistence.Migrations;

namespace IOKode.OpinionatedFramework.ContractImplementations.FluentMigrator;

/// <summary>
/// Applies migrations through a FluentMigrator runner.
/// </summary>
internal sealed class FluentMigratorMigrator(IMigrationRunner migrationRunner) : IMigrator
{
    public Task MigrateUpAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        migrationRunner.MigrateUp();
        return Task.CompletedTask;
    }
}
