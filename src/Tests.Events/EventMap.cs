using System;
using System.Linq.Expressions;
using FluentNHibernate.Mapping;
using IOKode.OpinionatedFramework.Events;

namespace IOKode.OpinionatedFramework.Tests.Events;

public class EventMap : ClassMap<Event>
{
    public EventMap()
    {
        Not.LazyLoad();
        Table("Events");
        Id<string>(column: "id").GeneratedBy.UuidString();
        Map(x => x.DispatchedAt).Column("dispatched_at").Nullable();
        Map(PayloadPropertyExpression()).Column("payload").Access.Using<EventPayloadAccessor>().CustomType<JsonBType>();

        DiscriminateSubClassesOnColumn("event_type");
    }

    private Expression<Func<Event, object>> PayloadPropertyExpression()
    {
        var eventType = typeof(Event);
        var eventPayloadType = typeof(EventPayloadAccessor);

        var eventTypeParams = Expression.Parameter(eventType, "x");
        var eventPayloadTypeParams = Expression.Parameter(eventPayloadType, "x");
        Expression expression = Expression.PropertyOrField(eventPayloadTypeParams, nameof(EventPayloadAccessor.Payload));

        return (Expression<Func<Event, object>>) Expression.Lambda(typeof(Func<Event, object>), expression, eventTypeParams);
    }
}

public class EventSubclassMap<T> : SubclassMap<T> where T : Event
{
    public EventSubclassMap()
    {
        Not.LazyLoad();
        DiscriminatorValue(typeof(T).FullName);
    }
}