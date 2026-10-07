namespace IOKode.OpinionatedFramework.Events;

/// <summary>
/// An actionable occurrence in the application, carried between the code that raises it and the code that
/// reacts to it.
/// </summary>
/// <remarks>
/// An event carries data and nothing else. It is not a persistence entity and holds no invariants of its own,
/// so it is neither tracked nor saved by a unit of work.
/// </remarks>
/// <remarks>
/// This is not implemented directly. An event declares what may be done with it by implementing
/// <see cref="IPublishableEvent"/>, <see cref="ISubscribableEvent"/>, or both, and a type implementing
/// neither reaches no API.
/// </remarks>
/// <remarks>
/// Every concrete event must be serializable and must declare an <see cref="EventNameAttribute"/>, because a
/// dispatcher may carry it across a process boundary or store it beyond the lifetime of the running assembly.
/// </remarks>
public interface IEvent;
