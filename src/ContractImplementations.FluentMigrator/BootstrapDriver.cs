using System.Collections.Generic;
using System.Linq;
using FluentMigrator.Runner;
using IOKode.OpinionatedFramework.ContractImplementations.FluentMigrator;
using IOKode.OpinionatedFramework.Drivers.Abstractions;
using IOKode.OpinionatedFramework.Persistence.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

[assembly: BootstrapDriver<IMigrator, FluentMigratorBootstrapDriver>("Migrations", "fluent-migrator")]

namespace IOKode.OpinionatedFramework.ContractImplementations.FluentMigrator;

/// <summary>
/// Registers the FluentMigrator implementation of the migration capability.
/// </summary>
/// <remarks>
/// Migration execution is independent of the unit-of-work and query-execution capabilities, so this driver is
/// selected on its own and needs no persistence driver.
/// </remarks>
public sealed class FluentMigratorBootstrapDriver : IBootstrapDriverRegistrar
{
    public static BootstrapValidationResult Validate(BootstrapDriverContext context)
    {
        var errors = new List<BootstrapValidationError>();
        var connectionStringName = context.DriverConfiguration["ConnectionStringName"];
        if (string.IsNullOrWhiteSpace(connectionStringName))
        {
            errors.Add(new BootstrapValidationError(
                $"{context.DriverConfiguration.Path}:ConnectionStringName",
                "A value is required."));
        }
        else if (string.IsNullOrWhiteSpace(context.Configuration.GetConnectionString(connectionStringName)))
        {
            errors.Add(new BootstrapValidationError(
                $"ConnectionStrings:{connectionStringName}",
                "The connection string is not configured."));
        }

        var applyOnStartup = context.DriverConfiguration["ApplyOnStartup"];
        if (!string.IsNullOrWhiteSpace(applyOnStartup) && !bool.TryParse(applyOnStartup, out _))
        {
            errors.Add(new BootstrapValidationError(
                $"{context.DriverConfiguration.Path}:ApplyOnStartup",
                "The value must be true or false."));
        }

        if (GetOptions(context).MigrationAssemblies.Count == 0)
        {
            errors.Add(new BootstrapValidationError(
                $"{context.DriverConfiguration.Path}:Driver",
                "At least one migration assembly is required. Add it with " +
                "options.FluentMigrator(migrations => migrations.AddMigrationAssembly(...))."));
        }

        return new BootstrapValidationResult(errors);
    }

    public static void Register(BootstrapDriverContext context)
    {
        var connectionString = context.Configuration
            .GetConnectionString(context.DriverConfiguration["ConnectionStringName"]!)!;
        var assemblies = GetOptions(context).MigrationAssemblies.ToArray();

        context.Services
            .AddFluentMigratorCore()
            .ConfigureRunner(builder => builder
                .AddPostgres()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(assemblies).For.Migrations());
        context.Services.AddTransient<IMigrator, FluentMigratorMigrator>();

        if (bool.TryParse(context.DriverConfiguration["ApplyOnStartup"], out var applyOnStartup) && applyOnStartup)
        {
            context.Services.AddSingleton<IHostedService, FluentMigratorHostedService>();
        }
    }

    private static FluentMigratorOptions GetOptions(BootstrapDriverContext context)
    {
        var options = new FluentMigratorOptions();
        context.GetOptionsConfigurator<FluentMigratorOptions>()?.Invoke(options);
        return options;
    }
}
