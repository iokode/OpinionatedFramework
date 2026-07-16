using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentMigrator.Runner;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.Bootstrapping.Abstractions;
using IOKode.OpinionatedFramework.Persistence.Queries;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: BootstrapDriver<IUnitOfWorkFactory,
    IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres.NHibernatePostgresBootstrapDriver>(
    "Persistence", "nhibernate-postgres")]
[assembly: BootstrapDriver<IQueryExecutor,
    IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres.NHibernatePostgresBootstrapDriver>(
    "Persistence", "nhibernate-postgres")]

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres;

public sealed class NHibernatePostgresBootstrapDriver : IBootstrapDriverRegistrar
{
    private const string RegistrationStateKey = "OpinionatedFramework.NHibernate.Postgres";

    public static BootstrapValidationResult Validate(BootstrapDriverContext context)
    {
        var errors = new List<BootstrapValidationError>();
        var connectionStringName = context.DriverConfiguration["ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionStringName))
        {
            errors.Add(new BootstrapValidationError(
                $"{context.DriverConfiguration.Path}:ConnectionString",
                "A value is required."));
        }
        else if (string.IsNullOrWhiteSpace(context.Configuration.GetConnectionString(connectionStringName)))
        {
            errors.Add(new BootstrapValidationError(
                $"ConnectionStrings:{connectionStringName}",
                "The connection string is not configured."));
        }

        var assemblySections = context.DriverConfiguration.GetSection("Assemblies").GetChildren().ToArray();
        if (assemblySections.Length == 0)
        {
            errors.Add(new BootstrapValidationError(
                $"{context.DriverConfiguration.Path}:Assemblies",
                "At least one assembly is required."));
        }

        foreach (var assemblySection in assemblySections)
        {
            if (string.IsNullOrWhiteSpace(assemblySection.Value))
            {
                errors.Add(new BootstrapValidationError(assemblySection.Path, "An assembly name is required."));
                continue;
            }

            try
            {
                Assembly.Load(assemblySection.Value);
            }
            catch (Exception exception) when (exception is FileNotFoundException or FileLoadException or BadImageFormatException)
            {
                errors.Add(new BootstrapValidationError(
                    assemblySection.Path,
                    $"Assembly '{assemblySection.Value}' could not be loaded."));
            }
        }

        return new BootstrapValidationResult(errors);
    }

    public static void Register(BootstrapDriverContext context)
    {
        context.GetOrAddSharedState(RegistrationStateKey, () =>
        {
            RegisterServices(context);
            return new RegistrationState();
        });
    }

    private static void RegisterServices(BootstrapDriverContext context)
    {
        var connectionStringName = context.DriverConfiguration["ConnectionString"]!;
        var connectionString = context.Configuration.GetConnectionString(connectionStringName)!;
        var assemblies = context.DriverConfiguration.GetSection("Assemblies").GetChildren()
            .Select(section => Assembly.Load(section.Value!))
            .ToArray();

        context.Services.AddNHibernateWithPostgres(configuration =>
        {
            var fluentConfiguration = Fluently.Configure(configuration)
                .Database(PostgreSQLConfiguration.PostgreSQL83.ConnectionString(connectionString));
            foreach (var assembly in assemblies)
            {
                fluentConfiguration.Mappings(mappings => mappings.FluentMappings.AddFromAssembly(assembly));
            }

            fluentConfiguration.BuildConfiguration();
        });

        context.Services
            .AddFluentMigratorCore()
            .ConfigureRunner(builder => builder
                .AddPostgres()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(assemblies).For.Migrations());

        if (bool.TryParse(context.DriverConfiguration["Migrations:ApplyOnStartup"], out var applyMigrations) &&
            applyMigrations)
        {
            context.Services.AddSingleton<IStartupTask, MigrationStartupTask>();
        }
    }

    private sealed class RegistrationState;
}

public sealed class MigrationStartupTask(IMigrationRunner migrationRunner) : IStartupTask
{
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        migrationRunner.MigrateUp();
        return Task.CompletedTask;
    }
}
