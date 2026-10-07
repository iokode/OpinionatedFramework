using System;
using IOKode.OpinionatedFramework.Drivers.Abstractions;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire;

/// <summary>
/// Contributes the Hangfire configuration verb to bootstrap options.
/// </summary>
public static class BootstrapOptionsExtensions
{
    /// <summary>
    /// Configures the <c>Hangfire</c> driver with settings that cannot be expressed in configuration.
    /// </summary>
    /// <remarks>
    /// Referencing this package makes the verb available, which is not the same as its driver being selected.
    /// Bootstrap fails when the <c>Hangfire</c> driver is selected for neither the job enqueuer nor the job
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

    /// <summary>
    /// Declares the events raised and the handlers run by the <c>Hangfire</c> event dispatcher driver.
    /// </summary>
    /// <remarks>
    /// Event declarations live in their own options type rather than in <see cref="HangfireOptions"/>, because
    /// they describe the application rather than the Hangfire setup, and because they must keep the same shape
    /// across every event dispatcher driver so switching driver does not change what a policy means.
    /// </remarks>
    /// <example>
    /// <code>
    /// options.HangfireEvents(events =>
    /// {
    ///     events.Publishes&lt;OrderSubmitted&gt;();
    ///     events.Handles&lt;OrderSubmitted, SendConfirmationEmail&gt;();
    ///     events.Handles&lt;OrderSubmitted, UpdateStatistics&gt;(policy => policy.Retry(3));
    /// });
    /// </code>
    /// </example>
    /// <param name="options">The bootstrap options.</param>
    /// <param name="configure">Declares the events and their handlers.</param>
    /// <returns>The same options, to allow chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static IBootstrapOptions HangfireEvents(this IBootstrapOptions options,
        Action<HangfireEventsOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Configure(configure);
        return options;
    }
}
