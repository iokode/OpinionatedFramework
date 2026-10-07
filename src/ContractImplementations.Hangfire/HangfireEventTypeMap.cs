namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire;

/// <summary>
/// Holds the event type map built from the declared events, so a running job can read an event back.
/// </summary>
/// <param name="Value">The map from declared event name to type.</param>
public sealed record HangfireEventTypeMap(EventTypeMap Value);
