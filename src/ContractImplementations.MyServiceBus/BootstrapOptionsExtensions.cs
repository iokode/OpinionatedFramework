using System;
using IOKode.OpinionatedFramework.Drivers.Abstractions;

namespace IOKode.OpinionatedFramework.ContractImplementations.MyServiceBus;

/// <summary>
/// Contributes the MyServiceBus events configuration verb to bootstrap options.
/// </summary>
public static class BootstrapOptionsExtensions
{
    /// <summary>
    /// Declares the events this process raises and the handlers it subscribes through a MyServiceBus event
    /// dispatcher driver.
    /// </summary>
    /// <remarks>
    /// Referencing this package makes the verb available, which is not the same as a driver being selected.
    /// Only what this process is responsible for is declared: a publishing process declares no handler, and a
    /// subscriber declares its own without the publisher knowing about them.
    /// </remarks>
    /// <remarks>
    /// The verb belongs to this package rather than to a transport package, because the declarations describe
    /// the application and are the same whichever broker carries them. Every MyServiceBus transport driver
    /// reads them, and only the selected one is ever asked for them.
    /// </remarks>
    /// <example>
    /// <code>
    /// options.MyServiceBusEvents(events =>
    /// {
    ///     events.Publishes&lt;OrderSubmitted&gt;();
    ///     events.Handles&lt;OrderSubmitted, SendConfirmationEmail&gt;(policy => policy.Retry(3));
    ///
    ///     // A handler written against the event interface is declared once per event it covers.
    ///     events.Handles&lt;OrderSubmitted, StoreEvent&gt;();
    ///     events.Handles&lt;InventoryAdjusted, StoreEvent&gt;();
    /// });
    /// </code>
    /// </example>
    /// <param name="options">The bootstrap options.</param>
    /// <param name="configure">Declares the events and their handlers.</param>
    /// <returns>The same options, to allow chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static IBootstrapOptions MyServiceBusEvents(this IBootstrapOptions options,
        Action<MyServiceBusEventsOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Configure(configure);
        return options;
    }
}
