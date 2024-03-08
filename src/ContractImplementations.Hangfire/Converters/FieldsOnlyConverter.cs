using System;
using System.Globalization;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Converters;

public class FieldsOnlyConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }
        
        if(value.GetType().IsPrimitive || value is string || value is decimal)
        {
            writer.WriteValue(value);
            return;
        }
        
        JObject jObject = new JObject();
        var fields = value.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
 
        foreach (var field in fields)
        {
            var itemValue = field.GetValue(value);
            jObject.Add(field.Name, JToken.FromObject(itemValue, serializer));
            // if (itemValue != null)
            // {
            //     if (itemValue is char charValue)
            //     {
            //         jObject.Add(field.Name, new JValue(charValue));
            //     }
            //     else if(itemValue.GetType().IsPrimitive || itemValue is string || itemValue is decimal)
            //     {
            //         jObject.Add(field.Name, new JValue(itemValue));
            //     }
            //     else
            //     {
            //         jObject.Add(field.Name, JToken.FromObject(itemValue, serializer));
            //     }
            // }
        }
 
        jObject.WriteTo(writer);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);
        var targetObj = Activator.CreateInstance(objectType, true);
        var fields = objectType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
 
        foreach (var field in fields)
        {
            var cleanName = field.Name;
            if (jObject[cleanName] != null)
            {
                var itemValue = jObject[cleanName].ToObject(field.FieldType, serializer);
                field.SetValue(targetObj, itemValue);
            }
        }
 
        return targetObj;
    }

    public override bool CanConvert(Type objectType)
    {
        return true;
    }
}