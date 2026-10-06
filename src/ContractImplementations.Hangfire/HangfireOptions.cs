using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire;

/// <summary>
/// Collects the code-level configuration of the <c>Hangfire</c> driver.
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
    private readonly Dictionary<string, List<Action<BackgroundJobServerOptions>>> namedServerConfigurators =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Adds a delegate that configures Hangfire itself.
    /// </summary>
    /// <remarks>
    /// This is where the job storage is supplied, along with the serializer settings and any global filter. A
    /// storage is selected by type and configured with a builder, so it cannot be expressed in configuration.
    /// The delegate runs on <see cref="GlobalConfiguration.Configuration"/> during bootstrap, before the servers
    /// are registered, so the enqueuer, the scheduler and every server observe the configured storage.
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
    /// Adds a delegate that configures every background job server the driver starts.
    /// </summary>
    /// <remarks>
    /// The delegate runs on options already carrying the <c>Queues</c>, <c>WorkerCount</c> and
    /// <c>ShutdownTimeout</c> values read from that server's entry of <c>Hangfire:Servers</c>, so it can both
    /// override them and set what configuration cannot express, such as a filter provider or a task scheduler.
    /// Settings a delegate does not touch keep the configured value. Use this overload for what every server
    /// shares and <see cref="ConfigureServer(string,Action{BackgroundJobServerOptions})"/> for one server. No
    /// delegate runs when <c>Hangfire:Servers</c> is absent or empty, because no server is created.
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
    /// Adds a delegate that configures the background job server named <paramref name="serverName"/>.
    /// </summary>
    /// <remarks>
    /// The name is the key of the server's entry in <c>Hangfire:Servers</c>, matched the way configuration keys
    /// are, without regard to case. The delegate runs after the ones added for every server, so a setting can be
    /// given a shared default and then refined for one server. Bootstrap fails when no entry carries the name,
    /// because the configuration would otherwise be discarded.
    /// </remarks>
    /// <example>
    /// <code>
    /// hangfire.ConfigureServer("events", server => server.SchedulePollingInterval = TimeSpan.FromSeconds(1));
    /// </code>
    /// </example>
    /// <param name="serverName">The name of the server to configure.</param>
    /// <param name="configure">Configures the background job server options.</param>
    /// <exception cref="ArgumentNullException"><paramref name="configure"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="serverName"/> is empty or whitespace.</exception>
    public void ConfigureServer(string serverName, Action<BackgroundJobServerOptions> configure)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serverName);
        ArgumentNullException.ThrowIfNull(configure);

        if (!this.namedServerConfigurators.TryGetValue(serverName, out var configurators))
        {
            configurators = new List<Action<BackgroundJobServerOptions>>();
            this.namedServerConfigurators.Add(serverName, configurators);
        }

        configurators.Add(configure);
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

    /// <summary>
    /// Gets the names the application configured that <paramref name="configuredServerNames"/> does not contain.
    /// </summary>
    internal IEnumerable<string> GetConfiguredServerNamesNotIn(IReadOnlyCollection<string> configuredServerNames)
    {
        return this.namedServerConfigurators.Keys.Where(serverName =>
            !configuredServerNames.Contains(serverName, StringComparer.OrdinalIgnoreCase));
    }

    internal void ApplyServerConfigurators(string serverName, BackgroundJobServerOptions serverOptions)
    {
        foreach (var configure in this.serverConfigurators)
        {
            configure(serverOptions);
        }

        if (this.namedServerConfigurators.TryGetValue(serverName, out var configurators))
        {
            foreach (var configure in configurators)
            {
                configure(serverOptions);
            }
        }
    }
}
