using System;

namespace IOKode.OpinionatedFramework.Foundation.Events;

public abstract record Event
{
    public required DateTimeOffset DispatchedAt { get; init; }
}