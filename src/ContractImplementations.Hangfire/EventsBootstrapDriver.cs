using System.Collections.Generic;
using System.Linq;
using IOKode.OpinionatedFramework.Drivers.Abstractions;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Internals.Events;

[assembly: BootstrapDriver<IEventDispatcher,
    IOKode.OpinionatedFramework.ContractImplementations.Hangfire.HangfireEventDispatcherBootstrapDriver>(
    "Events", "hangfire")]

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire;

/// <summary>
/// Registers the Hangfire-backed event dispatcher when the <c>hangfire</c> driver is selected for events.
/// </summary>
/// <remarks>
/// Selecting this driver requires a Hangfire server processing the configured queue. Without one the events are
/// stored and never handled, which is why the queue is named in configuration next to the server that reads it.
/// </remarks>
public sealed class HangfireEventDispatcherBootstrapDriver : IBootstrapDriverRegistrar
{
    public static BootstrapValidationResult Validate(BootstrapDriverContext context)
    {
        var options = new HangfireEventsOptions();
        context.GetOptionsConfigurator<HangfireEventsOptions>()?.Invoke(options);

        var errors = new List<BootstrapValidationError>(
            EventNameUniqueness.Validate(
                options.ResolveConcreteEventTypes(), context.DriverConfiguration.Path));
        errors.AddRange(HangfireBootstrapRegistration.Validate(context).Errors);

        return new BootstrapValidationResult(errors);
    }

    public static void Register(BootstrapDriverContext context)
    {
        var queueName = context.DriverConfiguration["Queue"];
        if (string.IsNullOrWhiteSpace(queueName))
        {
            queueName = HangfireEventsServiceExtensions.DefaultQueueName;
        }

        context.Services.AddHangfireEventDispatcher(
            queueName,
            context.GetOptionsConfigurator<HangfireEventsOptions>());
        HangfireBootstrapRegistration.Register(context);
    }
}
