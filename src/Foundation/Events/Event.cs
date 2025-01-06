using System;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;

namespace IOKode.OpinionatedFramework.Events;

public abstract class Event : Entity
{
    /// <remarks>Null if event was not dispatched. This property is set by the dispatcher.</remarks>
    public DateTimeOffset? DispatchedAt { get; private set; }
}