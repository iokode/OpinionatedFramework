using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Events;

/// <summary>
/// Defines a contract for dispatching events within the application.
/// </summary>
/// <remarks>
/// An implementation is responsible for getting the event to whatever reacts to it, and for the execution of
/// the handlers it runs itself. How far that reaches is the implementation's own: one runs every handler
/// declared in the process the event was dispatched in, and another hands the event to a broker that copies it
/// to subscribers this application knows nothing about.
/// </remarks>
/// <remarks>
/// What no implementation separates is delivery from execution, because whoever runs a handler is who decides
/// whether the attempt succeeded, and that decision drives retry and failure handling.
/// </remarks>
/// <remarks>
/// The contract promises the least an implementation can guarantee: when the returned task completes, the event
/// has been handed to the dispatcher. It does not promise that handlers have run, that they will run, or that
/// the event survives a process crash. Durability, ordering, and delivery guarantees are documented by each
/// implementation, so an application that depends on them must choose its implementation deliberately.
/// </remarks>
public interface IEventDispatcher
{
    /// <summary>
    /// Dispatches the specified event asynchronously.
    /// </summary>
    /// <remarks>
    /// Handlers are matched against the concrete runtime type of <paramref name="event"/>, so what reaches a
    /// handler is exactly what it was declared for.
    /// </remarks>
    /// <param name="event">
    /// The event to be dispatched. Only an event this application may raise is accepted, so the direction is
    /// enforced by the type system rather than checked when it is already too late.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the dispatch operation.
    /// Note that this token is used to cancel the dispatch process, not the
    /// execution of the event handlers.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous operation of dispatching
    /// the event. The task will complete when the event has been dispatched regardless
    /// the event handlers have been executed or not.
    /// </returns>
    /// <exception cref="MissingPublishDeclarationException">
    /// The concrete type of <paramref name="event"/> was not declared with
    /// <see cref="EventDeclarations{TPolicy}.Publishes{TEvent}"/>.
    /// </exception>
    public Task DispatchAsync(IPublishableEvent @event, CancellationToken cancellationToken);
}
