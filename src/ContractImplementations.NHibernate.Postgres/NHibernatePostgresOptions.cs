using System;
using System.Collections.Generic;
using System.Reflection;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate.QueryExecutor;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres;

/// <summary>
/// Collects the code-level configuration of the <c>nhibernate-postgres</c> driver.
/// </summary>
/// <remarks>
/// The driver supplies one options type covering everything it registers, rather than one bootstrap verb per
/// knob, so that a driver can grow new settings without adding extension methods whose signatures could collide
/// with those of another driver for the same contract.
/// </remarks>
public sealed class NHibernatePostgresOptions
{
    private readonly List<Assembly> mappingAssemblies = new();
    private readonly List<Action<PostgreSQLConfiguration>> databaseConfigurators = new();
    private readonly List<Action<MappingConfiguration>> mappingConfigurators = new();
    private readonly List<Action<global::NHibernate.Cfg.Configuration>> exposeConfigurationConfigurators = new();
    private readonly List<Action<global::NHibernate.Cfg.Configuration>> nHibernateConfigurators = new();
    private readonly List<Action<QueryExecutorOptions>> queryExecutorConfigurators = new();

    /// <summary>
    /// Adds an assembly whose Fluent mappings describe the application's entities.
    /// </summary>
    /// <remarks>
    /// An assembly is compile-time identity, so it is supplied here rather than through configuration, where a
    /// misspelled name would only fail once the application starts.
    /// </remarks>
    /// <example>
    /// <code>
    /// nhibernate.AddMappingAssembly(typeof(TaskItemMap).Assembly);
    /// </code>
    /// </example>
    /// <param name="assembly">The assembly containing Fluent mappings.</param>
    /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
    public void AddMappingAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        this.mappingAssemblies.Add(assembly);
    }

    internal IReadOnlyList<Assembly> MappingAssemblies => this.mappingAssemblies;

    /// <summary>
    /// Adds a delegate that configures the PostgreSQL persistence configurer.
    /// </summary>
    /// <remarks>
    /// The delegate runs on <see cref="PostgreSQLConfiguration.PostgreSQL83"/> already bound to the configured
    /// connection string, before it is handed to Fluent NHibernate, so it can select a connection provider or
    /// dialect, enable SQL logging, or otherwise refine the database configuration the driver builds by default.
    /// </remarks>
    /// <example>
    /// <code>
    /// nhibernate.ConfigureDatabase(postgres => postgres.Provider&lt;NpgsqlDataSourceConnectionProvider&gt;());
    /// </code>
    /// </example>
    /// <param name="configure">Configures the PostgreSQL persistence configurer.</param>
    /// <exception cref="ArgumentNullException"><paramref name="configure"/> is <see langword="null"/>.</exception>
    public void ConfigureDatabase(Action<PostgreSQLConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        this.databaseConfigurators.Add(configure);
    }

    /// <summary>
    /// Adds a delegate that configures the Fluent mapping container.
    /// </summary>
    /// <remarks>
    /// The delegate runs after the driver adds the mapping assemblies, within the same Fluent mappings stage, so
    /// it can register conventions, or add individual mapping types that no assembly scan can supply, such as a
    /// mapping constructed at runtime from an open generic.
    /// </remarks>
    /// <example>
    /// <code>
    /// nhibernate.ConfigureMappings(mappings =>
    /// {
    ///     mappings.FluentMappings.Conventions.AddFromAssemblyOf&lt;LowercaseConvention&gt;();
    ///     mappings.FluentMappings.Add(typeof(EventSubclassMap&lt;&gt;).MakeGenericType(subclass));
    /// });
    /// </code>
    /// </example>
    /// <param name="configure">Configures the Fluent mapping container.</param>
    /// <exception cref="ArgumentNullException"><paramref name="configure"/> is <see langword="null"/>.</exception>
    public void ConfigureMappings(Action<MappingConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        this.mappingConfigurators.Add(configure);
    }

    /// <summary>
    /// Adds a delegate that inspects or alters the built NHibernate configuration inside the Fluent pipeline.
    /// </summary>
    /// <remarks>
    /// The delegate runs while Fluent NHibernate builds the configuration, so unlike <see cref="ConfigureNHibernate"/>
    /// it can contribute to the pipeline itself, for example registering event listeners the mappings depend on.
    /// </remarks>
    /// <example>
    /// <code>
    /// nhibernate.ExposeConfiguration(cfg =>
    ///     cfg.EventListeners.PostLoadEventListeners = [new TemplatePostLoadEventListener()]);
    /// </code>
    /// </example>
    /// <param name="configure">Inspects or alters the NHibernate configuration.</param>
    /// <exception cref="ArgumentNullException"><paramref name="configure"/> is <see langword="null"/>.</exception>
    public void ExposeConfiguration(Action<global::NHibernate.Cfg.Configuration> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        this.exposeConfigurationConfigurators.Add(configure);
    }

    /// <summary>
    /// Adds a delegate that configures NHibernate itself.
    /// </summary>
    /// <remarks>
    /// The delegate runs after the Fluent pipeline has built the configuration, so it can override the connection
    /// string and mappings the driver applied. Use <see cref="ExposeConfiguration"/> instead when the delegate has
    /// to run inside the pipeline.
    /// </remarks>
    /// <param name="configure">Configures the NHibernate configuration.</param>
    /// <exception cref="ArgumentNullException"><paramref name="configure"/> is <see langword="null"/>.</exception>
    public void ConfigureNHibernate(Action<global::NHibernate.Cfg.Configuration> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        this.nHibernateConfigurators.Add(configure);
    }

    /// <summary>
    /// Adds a delegate that configures the query executor.
    /// </summary>
    /// <remarks>
    /// A query middleware is selected by type, so it can only be added from code.
    /// </remarks>
    /// <example>
    /// <code>
    /// nhibernate.ConfigureQueryExecutor(queries => queries.AddMiddleware&lt;QueryLoggingMiddleware&gt;());
    /// </code>
    /// </example>
    /// <param name="configure">Configures the query executor options.</param>
    /// <exception cref="ArgumentNullException"><paramref name="configure"/> is <see langword="null"/>.</exception>
    public void ConfigureQueryExecutor(Action<QueryExecutorOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        this.queryExecutorConfigurators.Add(configure);
    }

    internal void ApplyDatabaseConfigurators(PostgreSQLConfiguration database)
    {
        foreach (var configure in this.databaseConfigurators)
        {
            configure(database);
        }
    }

    internal void ApplyMappingConfigurators(MappingConfiguration mappings)
    {
        foreach (var configure in this.mappingConfigurators)
        {
            configure(mappings);
        }
    }

    internal void ApplyExposeConfigurationConfigurators(FluentConfiguration fluentConfiguration)
    {
        foreach (var configure in this.exposeConfigurationConfigurators)
        {
            fluentConfiguration.ExposeConfiguration(configure);
        }
    }

    internal void ApplyNHibernateConfigurators(global::NHibernate.Cfg.Configuration configuration)
    {
        foreach (var configure in this.nHibernateConfigurators)
        {
            configure(configuration);
        }
    }

    internal void ApplyQueryExecutorConfigurators(QueryExecutorOptions queryExecutorOptions)
    {
        foreach (var configure in this.queryExecutorConfigurators)
        {
            configure(queryExecutorOptions);
        }
    }
}
