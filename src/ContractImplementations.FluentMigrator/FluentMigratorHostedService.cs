using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Persistence.Migrations;
using Microsoft.Extensions.Hosting;

namespace IOKode.OpinionatedFramework.ContractImplementations.FluentMigrator;

/// <summary>
/// Applies pending migrations while the host starts.
/// </summary>
/// <remarks>
/// Registered only when the migration capability is configured with <c>ApplyOnStartup</c>.
/// </remarks>
internal sealed class FluentMigratorHostedService(IMigrator migrator) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        return migrator.MigrateUpAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
