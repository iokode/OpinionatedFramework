using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using IOKode.OpinionatedFramework.Commands;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MvcJsonOptions = Microsoft.AspNetCore.Mvc.JsonOptions;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations;

/// <summary>
/// Translates an <see cref="InvalidInputException"/> into the response of a controller.
/// </summary>
public static class InvalidInputControllerExtensions
{
    /// <summary>
    /// Creates a <c>400</c> response with a <see cref="ValidationProblemDetails"/> that has an entry per path of the
    /// errors of <paramref name="exception"/>. Each entry is named after where its data came from in the request:
    /// <list type="bullet">
    /// <item>
    /// data that a route or query string parameter set, by the name of that parameter, as MVC names its own model
    /// errors (<c>code</c>, <c>filter.IsSingle</c>);
    /// </item>
    /// <item>
    /// any other data, from the JSON body, by its JSONPath (RFC 9535), with each property named as the serializer of MVC
    /// names properties and each key of a dictionary as it names dictionary keys (<c>$.passenger.document.number</c>,
    /// <c>$.seats[2].row</c>, <c>$.seatsByCode['a.b'].row</c>);
    /// </item>
    /// <item>an error about the whole command, under <c>$</c>, the root of the request.</item>
    /// </list>
    /// </summary>
    /// <param name="controller">The controller that invoked the command.</param>
    /// <param name="exception">The exception thrown because the data of the command is not valid.</param>
    /// <param name="requestParametersByProperty">
    /// The route and query string parameters of the action, by the name of the property of the command each one sets.
    /// </param>
    public static IActionResult InvalidInput(this ControllerBase controller, InvalidInputException exception,
        IReadOnlyDictionary<string, string> requestParametersByProperty)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(requestParametersByProperty);

        var jsonOptions = controller.HttpContext.RequestServices.GetRequiredService<IOptions<MvcJsonOptions>>().Value.JsonSerializerOptions;
        var modelState = new ModelStateDictionary();
        foreach (var error in exception.Errors)
        {
            var key = error switch
            {
                ObjectValidationError => "$",
                PathValidationError pathError => GetKey(pathError.Path, requestParametersByProperty, jsonOptions),
                _ => throw new InvalidOperationException($"Unknown validation error '{error.GetType()}'.")
            };
            modelState.AddModelError(key, error.Message);
        }

        return controller.ValidationProblem(modelState);
    }

    private static string GetKey(ValidationPath path, IReadOnlyDictionary<string, string> requestParametersByProperty,
        JsonSerializerOptions jsonOptions)
    {
        if (path.Segments[0] is PropertyPathSegment first
            && requestParametersByProperty.TryGetValue(first.Name, out var parameterName))
        {
            return GetModelName(parameterName, path.Segments.Skip(1));
        }

        return GetJsonPath(path.Segments, jsonOptions);
    }

    /// <summary>
    /// The name MVC gives to the data of a model bound from the route or the query string.
    /// </summary>
    private static string GetModelName(string parameterName, IEnumerable<ValidationPathSegment> segments)
    {
        var builder = new StringBuilder(parameterName);
        foreach (var segment in segments)
        {
            switch (segment)
            {
                case PropertyPathSegment property:
                    builder.Append('.').Append(property.Name);
                    break;
                case IndexPathSegment index:
                    builder.Append('[').Append(index.Index.ToString(CultureInfo.InvariantCulture)).Append(']');
                    break;
                case KeyPathSegment key:
                    builder.Append('[').Append(FormatInvariant(key.Key)).Append(']');
                    break;
                default:
                    throw new InvalidOperationException($"Unknown validation path segment '{segment.GetType()}'.");
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// The JSONPath (RFC 9535) of the data in the JSON body: dot notation for a member name that it can hold, and
    /// bracket notation, escaped as a Normalized Path escapes it, for any other.
    /// </summary>
    private static string GetJsonPath(IEnumerable<ValidationPathSegment> segments, JsonSerializerOptions jsonOptions)
    {
        var builder = new StringBuilder("$");
        foreach (var segment in segments)
        {
            switch (segment)
            {
                case PropertyPathSegment property:
                    AppendMemberName(builder, jsonOptions.PropertyNamingPolicy?.ConvertName(property.Name) ?? property.Name);
                    break;
                case IndexPathSegment index:
                    builder.Append('[').Append(index.Index.ToString(CultureInfo.InvariantCulture)).Append(']');
                    break;
                case KeyPathSegment key:
                    AppendMemberName(builder, GetDictionaryKeyName(key.Key, jsonOptions));
                    break;
                default:
                    throw new InvalidOperationException($"Unknown validation path segment '{segment.GetType()}'.");
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// The member name the serializer writes for a key of a dictionary.
    /// </summary>
    private static string GetDictionaryKeyName(object key, JsonSerializerOptions jsonOptions)
    {
        var dictionary = (IDictionary) Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(key.GetType(), typeof(int)))!;
        dictionary.Add(key, 0);
        using var document = JsonSerializer.SerializeToDocument(dictionary, dictionary.GetType(), jsonOptions);
        return document.RootElement.EnumerateObject().Single().Name;
    }

    private static void AppendMemberName(StringBuilder builder, string name)
    {
        if (IsMemberNameShorthand(name))
        {
            builder.Append('.').Append(name);
            return;
        }

        builder.Append("['");
        foreach (var rune in name.EnumerateRunes())
        {
            switch (rune.Value)
            {
                case '\'':
                    builder.Append("\\'");
                    break;
                case '\\':
                    builder.Append("\\\\");
                    break;
                case '\b':
                    builder.Append("\\b");
                    break;
                case '\f':
                    builder.Append("\\f");
                    break;
                case '\n':
                    builder.Append("\\n");
                    break;
                case '\r':
                    builder.Append("\\r");
                    break;
                case '\t':
                    builder.Append("\\t");
                    break;
                case < 0x20:
                    builder.Append("\\u00").Append(rune.Value.ToString("x2", CultureInfo.InvariantCulture));
                    break;
                default:
                    builder.Append(rune.ToString());
                    break;
            }
        }

        builder.Append("']");
    }

    /// <summary>
    /// Whether <paramref name="name"/> matches <c>member-name-shorthand</c> of RFC 9535: a letter, <c>_</c> or a
    /// non-ASCII character, followed by any of them or digits.
    /// </summary>
    private static bool IsMemberNameShorthand(string name)
    {
        var isFirst = true;
        foreach (var rune in name.EnumerateRunes())
        {
            var value = rune.Value;
            var isNameFirst = value is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '_' or >= 0x80;
            var isDigit = value is >= '0' and <= '9';
            if (!(isNameFirst || (!isFirst && isDigit)))
            {
                return false;
            }

            isFirst = false;
        }

        return !isFirst;
    }

    private static string FormatInvariant(object value)
    {
        return value is IFormattable formattable
            ? formattable.ToString(null, CultureInfo.InvariantCulture)
            : value.ToString() ?? string.Empty;
    }
}
