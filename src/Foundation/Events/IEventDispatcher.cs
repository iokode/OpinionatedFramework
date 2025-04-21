using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Events;

/// <summary>
/// Defines a contract for dispatching events within the application.
/// </summary>
/// <remarks>
/// Implementations of this interface are responsible for managing and executing the full lifecycle
/// of an event dispatch process. This includes triggering event handlers, managing dependencies,
/// and ensuring that the appropriate operations are executed in response to the event.
/// </remarks>
public interface IEventDispatcher
{
    /// <summary>
    /// Dispatches the specified event asynchronously.
    /// </summary>
    /// <param name="event">
    /// The event to be dispatched. This event represents an occurrence or message
    /// within the application, which will be handled by appropriate handlers.
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
    public Task DispatchAsync(Event @event, CancellationToken cancellationToken);
}