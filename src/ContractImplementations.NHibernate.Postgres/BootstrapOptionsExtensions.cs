using System;
using IOKode.OpinionatedFramework.Drivers.Abstractions;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres;

/// <summary>
/// Contributes the NHibernate persistence configuration verb to bootstrap options.
/// </summary>
public static class BootstrapOptionsExtensions
{
    /// <summary>
    /// Configures the <c>nhibernate-postgres</c> driver with settings that cannot be expressed in configuration.
    /// </summary>
    /// <remarks>
    /// Bootstrap fails when the <c>nhibernate-postgres</c> driver is not selected, because the configuration
    /// would otherwise be discarded.
    /// </remarks>
    /// <example>
    /// <code>
    /// options.NHibernatePostgres(nhibernate =>
    /// {
    ///     nhibernate.ConfigureNHibernate(cfg => cfg.SetInterceptor(new AuditInterceptor()));
    ///     nhibernate.ConfigureQueryExecutor(queries => queries.AddMiddleware&lt;QueryLoggingMiddleware&gt;());
    /// });
    /// </code>
    /// </example>
    /// <param name="options">The bootstrap options.</param>
    /// <param name="configure">Configures the driver.</param>
    /// <returns>The same options, to allow chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static IBootstrapOptions NHibernatePostgres(this IBootstrapOptions options,
        Action<NHibernatePostgresOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Configure(configure);
        return options;
    }
}
