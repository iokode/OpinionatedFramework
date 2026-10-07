namespace IOKode.OpinionatedFramework.Events;

/// <summary>
/// An event this application may react to.
/// </summary>
/// <remarks>
/// Only a subscribable event can be given a handler, so reacting to an event the application is not meant to
/// react to does not compile.
/// </remarks>
public interface ISubscribableEvent : IEvent;
