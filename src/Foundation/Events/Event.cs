using IOKode.OpinionatedFramework.Persistence.UnitOfWork;
using NodaTime;

namespace IOKode.OpinionatedFramework.Events;

/// <summary>
/// Represents the base class for events within the OpinionatedFramework.
/// An event is an actionable occurrence or message in the application
/// that may be dispatched and handled by relevant components.
/// </summary>
/// <remarks>
/// This class inherits from the <see cref="Entity"/> class and provides the foundation
/// for event implementation. Events can be dispatched through an <see cref="IEventDispatcher"/>.
/// </remarks>
public abstract class Event : Entity
{
    /// <summary>
    /// The timestamp indicating when the event was dispatched.
    /// </summary>
    /// <remarks>
    /// This property is automatically set by the dispatcher during the dispatch process.
    /// Its value will remain null if the event has not been dispatched.
    /// </remarks>
    public Instant? DispatchedAt { get; private set; }

    /// <summary>
    /// Indicates whether the event has been dispatched.
    /// </summary>
    /// <remarks>
    /// The property returns true if the event's dispatch timestamp has been set,
    /// signaling that it has been processed by the dispatcher.
    /// It will return false otherwise.
    /// </remarks>
    public bool IsDispatched => DispatchedAt != null;
}