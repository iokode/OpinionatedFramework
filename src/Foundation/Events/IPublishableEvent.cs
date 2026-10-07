namespace IOKode.OpinionatedFramework.Events;

/// <summary>
/// An event this application may raise.
/// </summary>
/// <remarks>
/// Only a publishable event can be handed to <see cref="IEventDispatcher"/>, so raising an event the
/// application is not meant to raise does not compile.
/// </remarks>
/// <remarks>
/// Direction belongs to the application, not to a shared contract: when another process reacts to this event,
/// it is that process's declaration of the type that marks it <see cref="ISubscribableEvent"/>.
/// </remarks>
public interface IPublishableEvent : IEvent;
