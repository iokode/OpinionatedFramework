using System;

namespace IOKode.OpinionatedFramework.Events;

public abstract record Event
{
    public required DateTimeOffset DispatchedAt { get; init; }
}