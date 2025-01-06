using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using IOKode.OpinionatedFramework.Events;
using Newtonsoft.Json;
using NHibernate.Engine;
using NHibernate.Properties;

namespace IOKode.OpinionatedFramework.Tests.Events;

class EventPayloadAccessor : IPropertyAccessor
{
    public string Payload { get; set; } = string.Empty;

    public IGetter GetGetter(Type theClass, string propertyName)
    {
        return new PayloadGetter();
    }

    public ISetter GetSetter(Type theClass, string propertyName)
    {
        return new PayloadSetter();
    }

    public bool CanAccessThroughReflectionOptimizer => true;
}

class PayloadGetter : IGetter
{
    public object Get(object target)
    {
        var eventType = target.GetType();
        var eventBaseType = typeof(Event);

        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
        var properties = eventType.GetProperties(flags)
            .Where(property => eventBaseType.GetProperty(property.Name) == null)
            .ToDictionary(property => property.Name, property => property.GetValue(target));

        var json = JsonConvert.SerializeObject(properties);
        return json;
    }

    public object GetForInsert(object owner, IDictionary mergeMap, ISessionImplementor session)
    {
        return Get(owner);
    }

    public Type ReturnType { get; } = typeof(string);
    public string PropertyName { get; } = nameof(EventPayloadAccessor.Payload);
    public MethodInfo Method { get; } = typeof(EventPayloadAccessor).GetProperty(nameof(EventPayloadAccessor.Payload))!.GetGetMethod()!;
}

class PayloadSetter : ISetter
{
    public void Set(object target, object value)
    {
        var json = (string) value;
        var deserializeObject = JsonConvert.DeserializeObject(json, target.GetType());

        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
        var properties = deserializeObject!.GetType().GetProperties(flags);
        foreach (var property in properties)
        {
            var setMethod = property.DeclaringType!.GetProperty(property.Name, flags)!.GetSetMethod(true)!;
            setMethod.Invoke(target, new object[] {property.GetValue(deserializeObject)!});
        }
    }

    public string PropertyName { get; } = nameof(EventPayloadAccessor.Payload);
    public MethodInfo Method { get; } = typeof(EventPayloadAccessor).GetProperty(nameof(EventPayloadAccessor.Payload))!.GetSetMethod()!;
}