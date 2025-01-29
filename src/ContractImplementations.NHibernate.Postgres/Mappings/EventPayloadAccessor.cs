using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using IOKode.OpinionatedFramework.Events;
using NHibernate.Engine;
using NHibernate.Properties;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres.Mappings;

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
    public Type ReturnType { get; } = typeof(string);
    public string PropertyName { get; } = nameof(EventPayloadAccessor.Payload);
    public MethodInfo Method { get; } = typeof(EventPayloadAccessor).GetProperty(nameof(EventPayloadAccessor.Payload))!.GetGetMethod()!;

    public object Get(object target)
    {
        var eventType = target.GetType();
        var eventBaseType = typeof(Event);

        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
        var fields = eventType.GetFields(flags)
            .Where(field => field.DeclaringType != eventBaseType)
            .ToDictionary(field => field.Name, field => field.GetValue(target));

        var json = JsonSerializer.Serialize(fields);
        return json;
    }

    public object GetForInsert(object owner, IDictionary mergeMap, ISessionImplementor session)
    {
        return Get(owner);
    }
}

class PayloadSetter : ISetter
{
    public string PropertyName { get; } = nameof(EventPayloadAccessor.Payload);
    public MethodInfo Method { get; } = typeof(EventPayloadAccessor).GetProperty(nameof(EventPayloadAccessor.Payload))!.GetSetMethod()!;

    public void Set(object target, object value)
    {
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
        string jsonText = (string) value;

        using var document = JsonDocument.Parse(jsonText);
        foreach (var jsonProperty in document.RootElement.EnumerateObject())
        {
            string fieldName = jsonProperty.Name;
            var fieldInfo = target.GetType().GetField(fieldName, flags);
            if (fieldInfo == null)
            {
                throw new Exception("Invalid payload json.");
            }

            var fieldValue = ExtractValueFromJsonElement(jsonProperty.Value, fieldInfo.FieldType);
            fieldInfo.SetValue(target, fieldValue);
        }
    }

    private static object? ExtractValueFromJsonElement(JsonElement jsonElement, Type targetType)
    {
        var serializedJson = jsonElement.GetRawText();
        return JsonSerializer.Deserialize(serializedJson, targetType);
    }
}