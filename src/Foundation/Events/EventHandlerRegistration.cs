using System;
using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Events;

/// <summary>
/// Associates one handler type with the event type it handles, together with the policy its driver applies.
/// </summary>
/// <remarks>
/// <see cref="Invoke"/> is built where the handler is declared, which is the only place where both types are
/// known statically. A driver iterating over registrations therefore calls the handler through a typed
/// delegate instead of reconstructing the call from <see cref="Type"/> values it was handed.
/// </remarks>
/// <typeparam name="TPolicy">The execution policy owned by the driver.</typeparam>
/// <param name="EventType">The concrete event type the handler is declared for.</param>
/// <param name="HandlerType">The handler type implementing <see cref="IEventHandler{TEvent}"/>.</param>
/// <param name="Policy">How the driver executes the handler.</param>
/// <param name="Invoke">Resolves the handler from the supplied provider and calls it with the event.</param>
public sealed record EventHandlerRegistration<TPolicy>(
    Type EventType,
    Type HandlerType,
    TPolicy Policy,
    Func<IServiceProvider, IEvent, CancellationToken, Task> Invoke);
