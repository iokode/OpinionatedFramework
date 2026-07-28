using System;
using IOKode.OpinionatedFramework.Drivers.Abstractions;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire;

/// <summary>
/// Contributes the Hangfire configuration verb to bootstrap options.
/// </summary>
public static class BootstrapOptionsExtensions
{
    /// <summary>
    /// Configures the <c>hangfire</c> driver with settings that cannot be expressed in configuration.
    /// </summary>
    /// <remarks>
    /// Referencing this package makes the verb available, which is not the same as its driver being selected.
    /// Bootstrap fails when the <c>hangfire</c> driver is selected for neither the job enqueuer nor the job
    /// scheduler, because the configuration would otherwise be discarded. The verb is named after the driver
    /// rather than after a contract, because one driver serves both contracts.
    /// </remarks>
    /// <example>
    /// The storage is supplied here, so nothing about Hangfire has to be registered outside the bootstrap.
    /// <code>
    /// options.Hangfire(hangfire =>
    /// {
    ///     hangfire.ConfigureHangfire(configuration => configuration
    ///         .UseRecommendedSerializerSettings()
    ///         .UsePostgreSqlStorage(postgres => postgres.UseNpgsqlConnection(connectionString)));
    ///
    ///     hangfire.ConfigureServer(server => server.StopTimeout = TimeSpan.FromSeconds(30));
    /// });
    /// </code>
    /// </example>
    /// <param name="options">The bootstrap options.</param>
    /// <param name="configure">Configures the driver.</param>
    /// <returns>The same options, to allow chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static IBootstrapOptions Hangfire(this IBootstrapOptions options, Action<HangfireOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Configure(configure);
        return options;
    }
}
