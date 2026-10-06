using System;
using IOKode.OpinionatedFramework.Drivers.Abstractions;

namespace IOKode.OpinionatedFramework.ContractImplementations.InMemoryEvents;

/// <summary>
/// Contributes the in-memory events configuration verb to bootstrap options.
/// </summary>
public static class BootstrapOptionsExtensions
{
    /// <summary>
    /// Configures the <c>InMemory</c> event dispatcher driver with settings that cannot be expressed in configuration.
    /// </summary>
    /// <remarks>
    /// Referencing this package makes the verb available, which is not the same as its driver being selected.
    /// Bootstrap fails when the <c>InMemory</c> driver is not the selected one, because the handlers declared
    /// here would otherwise be discarded. The driver is named after where it runs handlers rather than after
    /// being the default, so a configuration file states what it does and not merely that nothing else was
    /// chosen.
    /// </remarks>
    /// <example>
    /// A handler is selected by type, so it can only be declared from code.
    /// <code>
    /// options.InMemoryEvents(events =>
    /// {
    ///     events.AddEventHandler&lt;OrderSubmitted, SendConfirmationEmail&gt;();
    ///     events.AddEventHandler&lt;OrderSubmitted, UpdateStatistics&gt;(policy => policy.Retry(3));
    /// });
    /// </code>
    /// </example>
    /// <param name="options">The bootstrap options.</param>
    /// <param name="configure">Declares the handlers.</param>
    /// <returns>The same options, to allow chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static IBootstrapOptions InMemoryEvents(this IBootstrapOptions options,
        Action<InMemoryEventsOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Configure(configure);
        return options;
    }
}
