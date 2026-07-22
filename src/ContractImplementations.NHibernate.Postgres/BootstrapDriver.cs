using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres;
using IOKode.OpinionatedFramework.Drivers.Abstractions;
using IOKode.OpinionatedFramework.Persistence.Queries;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;
using Microsoft.Extensions.Configuration;

[assembly: BootstrapDriver<IUnitOfWorkFactory, NHibernatePostgresBootstrapDriver>("UnitOfWork", "nhibernate-postgres")]
[assembly: BootstrapDriver<IQueryExecutor, NHibernatePostgresBootstrapDriver>("QueryExecutor", "nhibernate-postgres")]

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres;

/// <summary>
/// Registers the NHibernate PostgreSQL implementation of the unit-of-work and query-execution contracts.
/// </summary>
/// <remarks>
/// The two contracts are independent capabilities, so an application can select this driver for either or both.
/// Each capability carries its own settings, and slots sharing a connection string share one session factory.
/// </remarks>
public sealed class NHibernatePostgresBootstrapDriver : IBootstrapDriverRegistrar
{
    private const string RegistrationStateKey = "OpinionatedFramework.NHibernate.Postgres";
    private const string UnitOfWorkSlotKey = "UnitOfWork";

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

        // Only the unit of work needs entity mappings. The query executor runs raw SQL and maps results through
        // the user-type registry, so it is usable without them.
        if (context.DriverConfiguration.Key == UnitOfWorkSlotKey && GetOptions(context).MappingAssemblies.Count == 0)
        {
            errors.Add(new BootstrapValidationError(
                $"{context.DriverConfiguration.Path}:Driver",
                "At least one mapping assembly is required. Add it with " +
                "options.NHibernatePostgres(nhibernate => nhibernate.AddMappingAssembly(...))."));
        }

        return new BootstrapValidationResult(errors);
    }

    public static void Register(BootstrapDriverContext context)
    {
        var connectionString = context.Configuration
            .GetConnectionString(context.DriverConfiguration["ConnectionStringName"]!)!;

        // Each slot carries its own connection, so slots pointing at the same database share one session factory
        // while slots pointing at different databases, such as a read replica, get one each.
        context.GetOrAddSharedState($"{RegistrationStateKey}:{connectionString}", () =>
        {
            RegisterServices(context, connectionString);
            return new RegistrationState();
        });
    }

    private static NHibernatePostgresOptions GetOptions(BootstrapDriverContext context)
    {
        var options = new NHibernatePostgresOptions();
        context.GetOptionsConfigurator<NHibernatePostgresOptions>()?.Invoke(options);
        return options;
    }

    private static void RegisterServices(BootstrapDriverContext context, string connectionString)
    {
        var driverOptions = GetOptions(context);
        var assemblies = driverOptions.MappingAssemblies.ToArray();

        context.Services.AddNHibernateWithPostgres(configuration =>
        {
            var database = PostgreSQLConfiguration.PostgreSQL83.ConnectionString(connectionString);
            driverOptions.ApplyDatabaseConfigurators(database);

            var fluentConfiguration = Fluently.Configure(configuration).Database(database);
            fluentConfiguration.Mappings(mappings =>
            {
                foreach (var assembly in assemblies)
                {
                    mappings.FluentMappings.AddFromAssembly(assembly);
                }

                driverOptions.ApplyMappingConfigurators(mappings);
            });
            driverOptions.ApplyExposeConfigurationConfigurators(fluentConfiguration);

            fluentConfiguration.BuildConfiguration();
            driverOptions.ApplyNHibernateConfigurators(configuration);
        }, driverOptions.ApplyQueryExecutorConfigurators);
    }

    private sealed class RegistrationState;
}
