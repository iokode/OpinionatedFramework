namespace IOKode.OpinionatedFramework.ContractImplementations.MyServiceBus.RabbitMq;

/// <summary>
/// The broker connection the driver uses.
/// </summary>
/// <param name="Host">The RabbitMQ host name.</param>
/// <param name="Port">The RabbitMQ port.</param>
/// <param name="Username">The user name used to connect.</param>
/// <param name="Password">The password used to connect.</param>
public sealed record MyServiceBusConnection(string Host, int Port, string Username, string Password)
{
    /// <summary>The port used when configuration names none.</summary>
    public const int DefaultPort = 5672;
}
