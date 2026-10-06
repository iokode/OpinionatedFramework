using System;
using IOKode.OpinionatedFramework.Drivers.Abstractions;

namespace IOKode.OpinionatedFramework.ContractImplementations.MyServiceBus;

/// <summary>
/// Contributes the MyServiceBus events configuration verb to bootstrap options.
/// </summary>
public static class BootstrapOptionsExtensions
{
    /// <summary>
    /// Declares the handlers this process subscribes through a MyServiceBus event dispatcher driver.
    /// </summary>
    /// <remarks>
    /// Referencing this package makes the verb available, which is not the same as a driver being selected.
    /// Only the handlers this process is responsible for are declared: a publishing process declares none, and
    /// a subscriber declares its own without the publisher knowing about them.
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
    ///     events.AddEventHandler&lt;OrderSubmitted, SendConfirmationEmail&gt;(policy => policy.Retry(3));
    ///
    ///     // A handler registered against the event interface needs any concrete event no handler names.
    ///     events.AddEvent&lt;InventoryAdjusted&gt;();
    ///     events.AddEventHandler&lt;ISubscribableEvent, StoreEvent&gt;();
    /// });
    /// </code>
    /// </example>
    /// <param name="options">The bootstrap options.</param>
    /// <param name="configure">Declares the handlers.</param>
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
