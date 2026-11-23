using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using IOKode.OpinionatedFramework.Commands;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations.Json;

/// <summary>
/// Reads an <see cref="Updatable{T}"/>: a member that is absent is never read, so it stays not to be changed; a member
/// that is present, <see langword="null"/> included, is to be changed to that value.
/// </summary>
/// <remarks>
/// Writing an <see cref="Updatable{T}"/> is not supported: it expresses a change a command receives, and nothing
/// serializes commands.
/// </remarks>
public sealed class UpdatableJsonConverter : JsonConverterFactory
{
    /// <exception cref="ArgumentNullException"><paramref name="typeToConvert"/> is <see langword="null"/>.</exception>
    public override bool CanConvert(Type typeToConvert)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);
        return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Updatable<>);
    }

    /// <exception cref="ArgumentNullException"><paramref name="typeToConvert"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="typeToConvert"/> is not an <see cref="Updatable{T}"/>.</exception>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);
        if (!CanConvert(typeToConvert))
        {
            throw new ArgumentException(
                $"The type '{typeToConvert}' is not '{typeof(Updatable<>)}', the only type this converter reads.",
                nameof(typeToConvert));
        }

        var valueType = typeToConvert.GetGenericArguments()[0];
        return (JsonConverter) Activator.CreateInstance(typeof(UpdatableJsonConverter<>).MakeGenericType(valueType))!;
    }
}

internal sealed class UpdatableJsonConverter<T> : JsonConverter<Updatable<T>>
{
    public override bool HandleNull => true;

    public override Updatable<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new Updatable<T>(JsonSerializer.Deserialize<T>(ref reader, options)!);
    }

    public override void Write(Utf8JsonWriter writer, Updatable<T> value, JsonSerializerOptions options)
    {
        throw new NotSupportedException(
            $"Writing an '{typeof(Updatable<T>)}' is not supported: it expresses a change a command receives, and nothing serializes commands.");
    }
}
