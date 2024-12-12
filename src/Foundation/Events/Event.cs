using System;

namespace IOKode.OpinionatedFramework.Events;

public abstract class Event
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>Null if event was not dispatched. This property is set by the dispatcher.</remarks>
    public DateTimeOffset? DispatchedAt { get; private set; }
}