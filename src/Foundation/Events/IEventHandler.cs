using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Events;

/// <summary>
/// Reacts to an event this application subscribes.
/// </summary>
/// <remarks>
/// <typeparamref name="TEvent"/> is contravariant, so a handler written against a group of events — an event
/// interface, or <see cref="ISubscribableEvent"/> itself — is accepted wherever a handler of one of those
/// events is asked for. That is what lets a handler that stores or audits everything be declared for each
/// concrete event it covers, instead of being registered against the interface and expanded afterwards.
/// </remarks>
/// <typeparam name="TEvent">The event type this handler reacts to.</typeparam>
public interface IEventHandler<in TEvent> where TEvent : ISubscribableEvent
{
    public Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}
