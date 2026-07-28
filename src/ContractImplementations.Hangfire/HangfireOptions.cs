using System;
using System.Collections.Generic;
using Hangfire;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire;

/// <summary>
/// Collects the code-level configuration of the <c>hangfire</c> driver.
/// </summary>
/// <remarks>
/// The driver supplies one options type covering everything it registers, rather than one bootstrap verb per
/// knob, so that a driver can grow new settings without adding extension methods whose signatures could collide
/// with those of another driver for the same contract.
/// </remarks>
public sealed class HangfireOptions
{
    private readonly List<Action<IGlobalConfiguration>> hangfireConfigurators = new();
    private readonly List<Action<BackgroundJobServerOptions>> serverConfigurators = new();

    /// <summary>
    /// Adds a delegate that configures Hangfire itself.
    /// </summary>
    /// <remarks>
    /// This is where the job storage is supplied, along with the serializer settings and any global filter. A
    /// storage is selected by type and configured with a builder, so it cannot be expressed in configuration.
    /// The delegate runs on <see cref="GlobalConfiguration.Configuration"/> during bootstrap, before the worker
    /// is registered, so the enqueuer, the scheduler and the worker all observe the configured storage.
    /// </remarks>
    /// <example>
    /// <code>
    /// hangfire.ConfigureHangfire(configuration => configuration
    ///     .UseRecommendedSerializerSettings()
    ///     .UsePostgreSqlStorage(postgres => postgres.UseNpgsqlConnection(connectionString)));
    /// </code>
    /// </example>
    /// <param name="configure">Configures Hangfire.</param>
    /// <exception cref="ArgumentNullException"><paramref name="configure"/> is <see langword="null"/>.</exception>
    public void ConfigureHangfire(Action<IGlobalConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        this.hangfireConfigurators.Add(configure);
    }

    /// <summary>
    /// Adds a delegate that configures the background job server the worker starts.
    /// </summary>
    /// <remarks>
    /// The delegate runs on options already carrying the <c>Queues</c>, <c>WorkerCount</c> and
    /// <c>ShutdownTimeout</c> values read from the root <c>Hangfire</c> configuration section, so it can both
    /// override them and set what configuration cannot express, such as a filter provider or a task scheduler.
    /// Settings a delegate does not touch keep the configured value. The delegate is not invoked when
    /// <c>StartWorker</c> is not enabled, because no server is created.
    /// </remarks>
    /// <example>
    /// <code>
    /// hangfire.ConfigureServer(server => server.StopTimeout = TimeSpan.FromSeconds(30));
    /// </code>
    /// </example>
    /// <param name="configure">Configures the background job server options.</param>
    /// <exception cref="ArgumentNullException"><paramref name="configure"/> is <see langword="null"/>.</exception>
    public void ConfigureServer(Action<BackgroundJobServerOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        this.serverConfigurators.Add(configure);
    }

    /// <summary>
    /// Gets whether the application supplied at least one Hangfire configurator.
    /// </summary>
    internal bool HasHangfireConfigurators => this.hangfireConfigurators.Count > 0;

    internal void ApplyHangfireConfigurators(IGlobalConfiguration configuration)
    {
        foreach (var configure in this.hangfireConfigurators)
        {
            configure(configuration);
        }
    }

    internal void ApplyServerConfigurators(BackgroundJobServerOptions serverOptions)
    {
        foreach (var configure in this.serverConfigurators)
        {
            configure(serverOptions);
        }
    }
}
