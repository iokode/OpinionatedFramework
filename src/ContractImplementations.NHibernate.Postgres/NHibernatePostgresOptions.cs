using System;
using System.Collections.Generic;
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
    private readonly List<Action<global::NHibernate.Cfg.Configuration>> nHibernateConfigurators = new();
    private readonly List<Action<QueryExecutorOptions>> queryExecutorConfigurators = new();

    /// <summary>
    /// Adds a delegate that configures NHibernate itself.
    /// </summary>
    /// <remarks>
    /// The delegate runs after the driver applies the configured connection string and mapping assemblies, so it
    /// can override them.
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
