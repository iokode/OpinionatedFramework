using System;
using IOKode.OpinionatedFramework.Events;
using MyServiceBus;

namespace IOKode.OpinionatedFramework.ContractImplementations.MyServiceBus.RabbitMq;

/// <summary>
/// Names the exchange that carries an event after its declared name.
/// </summary>
/// <remarks>
/// The exchange is the meeting point of the publisher and the subscribers, so naming it after the declared
/// name is what lets an application react to an event another one raises while each declares its own CLR type
/// for it. The formatter governs both sides: the exchange an event is published to and the exchange a
/// handler's queue is bound to are resolved through it.
/// </remarks>
/// <remarks>
/// The bus also carries messages it invents, such as the fault it publishes when a delivery fails for good.
/// Those are not events and declare no name, so they keep the name MyServiceBus gives them.
/// </remarks>
internal sealed class EventExchangeName : IMessageEntityNameFormatter
{
    /// <inheritdoc/>
    public string FormatEntityName(Type messageType)
    {
        return EventName.Declared(messageType) ?? EntityNameFormatter.Format(messageType, null);
    }
}
