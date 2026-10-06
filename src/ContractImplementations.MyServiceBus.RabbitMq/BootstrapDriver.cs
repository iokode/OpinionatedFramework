using System.Collections.Generic;
using System.Globalization;
using IOKode.OpinionatedFramework.Drivers.Abstractions;
using IOKode.OpinionatedFramework.Events;
using IOKode.OpinionatedFramework.Internals.Events;

[assembly: BootstrapDriver<IEventDispatcher,
    IOKode.OpinionatedFramework.ContractImplementations.MyServiceBus.RabbitMq.MyServiceBusRabbitMqBootstrapDriver>(
    "Events", "MyServiceBus.RabbitMq")]

namespace IOKode.OpinionatedFramework.ContractImplementations.MyServiceBus.RabbitMq;

/// <summary>
/// Registers the RabbitMQ-carried event dispatcher when the <c>MyServiceBus.RabbitMq</c> driver is selected.
/// </summary>
/// <example>
/// <code>
/// "OpinionatedFramework": {
///   "Events": {
///     "Driver": "MyServiceBus.RabbitMq",
///     "Host": "localhost",
///     "Port": 5672,
///     "Username": "guest",
///     "Password": "guest"
///   }
/// }
/// </code>
/// </example>
public sealed class MyServiceBusRabbitMqBootstrapDriver : IBootstrapDriverRegistrar
{
    private const string DefaultUsername = "guest";
    private const string DefaultPassword = "guest";

    public static BootstrapValidationResult Validate(BootstrapDriverContext context)
    {
        var options = new MyServiceBusEventsOptions();
        context.GetOptionsConfigurator<MyServiceBusEventsOptions>()?.Invoke(options);

        var concreteEventTypes = options.ResolveConcreteEventTypes();

        var errors = new List<BootstrapValidationError>(
            EventNameUniqueness.Validate(concreteEventTypes, context.DriverConfiguration.Path));

        // The broker carries the event as JSON and the consumer rebuilds it, so an event that does not survive
        // that round trip is rejected now instead of landing in an error queue later.
        errors.AddRange(MyServiceBusEventSerialization.Validate(
            concreteEventTypes, context.DriverConfiguration.Path));

        if (string.IsNullOrWhiteSpace(context.DriverConfiguration["Host"]))
        {
            errors.Add(new BootstrapValidationError(
                $"{context.DriverConfiguration.Path}:Host",
                "A broker host is required by the MyServiceBus.RabbitMq driver."));
        }

        if (ReadPort(context) is null)
        {
            errors.Add(new BootstrapValidationError(
                $"{context.DriverConfiguration.Path}:Port",
                "The value must be a port number between 1 and 65535."));
        }

        return new BootstrapValidationResult(errors);
    }

    public static void Register(BootstrapDriverContext context)
    {
        var connection = new MyServiceBusConnection(
            context.DriverConfiguration["Host"]!,
            ReadPort(context) ?? MyServiceBusConnection.DefaultPort,
            context.DriverConfiguration["Username"] ?? DefaultUsername,
            context.DriverConfiguration["Password"] ?? DefaultPassword);

        context.Services.AddMyServiceBusRabbitMqEventDispatcher(
            connection,
            context.GetOptionsConfigurator<MyServiceBusEventsOptions>());
    }

    /// <summary>
    /// Reads the configured port, or the default when none is configured.
    /// </summary>
    /// <returns>The port, or <see langword="null"/> when the configured value is not a valid port.</returns>
    private static int? ReadPort(BootstrapDriverContext context)
    {
        var configuredValue = context.DriverConfiguration["Port"];
        if (string.IsNullOrWhiteSpace(configuredValue))
        {
            return MyServiceBusConnection.DefaultPort;
        }

        if (int.TryParse(configuredValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var port)
            && port is > 0 and <= 65535)
        {
            return port;
        }

        return null;
    }
}
