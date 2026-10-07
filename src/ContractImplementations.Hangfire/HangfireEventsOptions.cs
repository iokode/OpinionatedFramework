using IOKode.OpinionatedFramework.Events;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire;

/// <summary>
/// Declares the events this process raises and the handlers the Hangfire event dispatcher enqueues and runs.
/// </summary>
/// <remarks>
/// This driver rebuilds the event from the job payload, so every event it may be given is named here: a
/// handler declaration names the one it reacts to, and a publish declaration names the one it raises.
/// </remarks>
public sealed class HangfireEventsOptions : EventHandlerCollection<HangfireEventHandlerPolicy>;

/// <summary>
/// Holds the event type map built from the declared events, so a running job can read an event back.
/// </summary>
/// <param name="Value">The map from declared event name to type.</param>
public sealed record HangfireEventTypeMap(EventTypeMap Value);
