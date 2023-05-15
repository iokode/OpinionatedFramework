using System.Collections.Generic;
using IOKode.OpinionatedFramework.Events;

namespace IOKode.OpinionatedFramework.Persistence;

public abstract class AggregateRoot
{
    private readonly List<Event> _events = new();

    protected void AddEvent(Event @event)
    {
        _events.Add(@event);
    }

    public IEnumerable<Event> Events => _events;
}