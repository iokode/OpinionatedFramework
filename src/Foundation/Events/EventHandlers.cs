using System;
using System.Collections.Generic;
using System.Linq;

namespace IOKode.OpinionatedFramework.Events;

public static class EventHandlers
{
    private class _Map
    {
        public readonly Type EventType;
        public readonly List<Type> HandlerTypes = new();

        public _Map(Type tEvent)
        {
            EventType = tEvent;
        }
    }

    private static List<_Map> maps = new();

    public static void Register<TEvent, THandler>()
        where TEvent : Event
        where THandler : IEventHandler<TEvent>
    {
        var map = maps.FirstOrDefault(map => map.EventType == typeof(TEvent));

        if (map is null)
        {
            map = new _Map(typeof(TEvent));
            maps.Add(map);
        }

        map.HandlerTypes.Add(typeof(THandler));
    }

    public static IEnumerable<Type> GetHandlerTypes(Type eventType)
    {
        var maps = EventHandlers.maps.Where(map => map.EventType.IsAssignableFrom(eventType));
        var handlers = maps.SelectMany(map => map.HandlerTypes);
        return handlers;
    }
    
}