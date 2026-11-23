using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations;

/// <summary>
/// Reports, in the model state of an action that reads the data of a command from the query string into a type of its
/// own, what MVC accepts without reporting:
/// <list type="bullet">
/// <item>
/// each required value the query string does not have. A value is there when the query string has a value with its
/// name as a prefix, as MVC decides it when binding: <c>page</c> for a simple value, any <c>filter.*</c> for a complex
/// one, an empty value included. A value that is there but cannot be read is reported only by MVC.
/// </item>
/// <item>
/// each name the query string repeats when the value it names is not a collection (<c>page=1&amp;page=2</c>,
/// <c>filter.isSingle=true&amp;filter.isSingle=false</c>, <c>ids[0]=1&amp;ids[0]=2</c>), from which MVC would read the
/// first value and leave out the others. A collection takes a repeated name (<c>ids=1&amp;ids=2</c>), and so do the
/// <c>index</c> values of a collection (<c>ids.index=a&amp;ids.index=b</c>). A name that is no value of the type is left
/// to MVC, which does not read it.
/// </item>
/// <item>
/// each collection whose elements are given by index when the indexes are not 0, 1, 2… without gaps
/// (<c>ids[0]=1&amp;ids[2]=3</c>, <c>ids[1]=2</c>), from which MVC would read the elements before the first gap and
/// leave out the others. In a collection that is not a dictionary, every index between brackets is one; in a
/// dictionary, only one followed by <c>.key</c> or <c>.value</c> is (<c>weights[0].key=x&amp;weights[0].value=1</c>),
/// since <c>weights[1]=2</c> gives the value of the key <c>1</c>. A collection with <c>index</c> values names its own
/// indexes and is left to MVC.
/// </item>
/// </list>
/// </summary>
/// <remarks>
/// It runs before <see cref="ModelStateInvalidFilter"/>, so that the response of <c>[ApiController]</c> to an invalid
/// model state has these errors too.
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public sealed class CommandQueryStringAttribute : Attribute, IActionFilter, IOrderedFilter
{
    private static readonly Regex segmentPattern = new(@"\G(?:(?<name>[^.\[\]]+)|\.(?<name>[^.\[\]]+)|\[(?<index>[^\]]*)\])", RegexOptions.CultureInvariant);

    /// <param name="queryStringType">The type the action reads the query string into.</param>
    /// <param name="requiredNames">The names in the query string of the values the command requires.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="queryStringType"/> or <paramref name="requiredNames"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// One of <paramref name="requiredNames"/> is <see langword="null"/>, empty or white space.
    /// </exception>
    public CommandQueryStringAttribute(Type queryStringType, params string[] requiredNames)
    {
        ArgumentNullException.ThrowIfNull(queryStringType);
        ArgumentNullException.ThrowIfNull(requiredNames);
        for (var index = 0; index < requiredNames.Length; index++)
        {
            if (string.IsNullOrWhiteSpace(requiredNames[index]))
            {
                throw new ArgumentException(
                    $"The required name at index {index} is null, empty or white space; a required name is the name of a value in the query string.",
                    nameof(requiredNames));
            }
        }

        QueryStringType = queryStringType;
        RequiredNames = requiredNames;
    }

    /// <summary>
    /// The type the action reads the query string into.
    /// </summary>
    public Type QueryStringType { get; }

    /// <summary>
    /// The names in the query string of the values the command requires.
    /// </summary>
    public IReadOnlyCollection<string> RequiredNames { get; }

    /// <summary>
    /// <c>-3000</c>, before <see cref="ModelStateInvalidFilter"/>, which runs at <c>-2000</c>.
    /// </summary>
    public int Order => -3000;

    /// <exception cref="ArgumentNullException"><paramref name="context"/> is <see langword="null"/>.</exception>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var query = context.HttpContext.Request.Query;
        var values = new QueryStringValueProvider(BindingSource.Query, query, CultureInfo.InvariantCulture);
        foreach (var name in RequiredNames)
        {
            if (!values.ContainsPrefix(name))
            {
                context.ModelState.AddModelError(name, $"A value for '{name}' was not provided.");
            }
        }

        var root = context.HttpContext.RequestServices.GetRequiredService<IModelMetadataProvider>().GetMetadataForType(QueryStringType);
        var indexesByCollection = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, nameValues) in query)
        {
            var metadata = Walk(root, name, indexesByCollection);
            if (nameValues.Count > 1 && metadata is { IsEnumerableType: false })
            {
                context.ModelState.AddModelError(name, $"Multiple values are not accepted for '{name}'.");
            }
        }

        foreach (var (collection, indexes) in indexesByCollection)
        {
            if (query.ContainsKey($"{collection}.index"))
            {
                continue;
            }

            var distinctIndexes = indexes.Distinct(StringComparer.Ordinal).ToArray();
            var expectedIndexes = Enumerable.Range(0, distinctIndexes.Length).Select(index => index.ToString(CultureInfo.InvariantCulture));
            if (!distinctIndexes.Order(StringComparer.Ordinal).SequenceEqual(expectedIndexes.Order(StringComparer.Ordinal), StringComparer.Ordinal))
            {
                context.ModelState.AddModelError(collection, $"The indexes of '{collection}' are not consecutive numbers from 0.");
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    /// <summary>
    /// The metadata of the value <paramref name="name"/> names in the type <paramref name="root"/> describes, going
    /// through its segments as MVC does when binding: a property by its name in the query string, ignoring case; an
    /// element of a collection by an index or a key between brackets; and the key or the value of an element of a
    /// dictionary by <c>Key</c> or <c>Value</c>. <see langword="null"/> when <paramref name="name"/> names no value of
    /// <paramref name="root"/>, or names the <c>index</c> values of a collection. Each index of a collection the walk goes
    /// through is added to <paramref name="indexesByCollection"/> under the name of that collection.
    /// </summary>
    private static ModelMetadata? Walk(ModelMetadata root, string name, Dictionary<string, List<string>> indexesByCollection)
    {
        var matches = new List<Match>();
        for (var match = segmentPattern.Match(name); match.Success && match.Length > 0; match = segmentPattern.Match(name, match.Index + match.Length))
        {
            matches.Add(match);
        }

        if (matches.Sum(match => match.Length) != name.Length)
        {
            return null;
        }

        ModelMetadata? current = root;
        for (var position = 0; position < matches.Count && current is not null; position++)
        {
            var match = matches[position];
            if (match.Groups["index"].Success)
            {
                if (!current.IsEnumerableType)
                {
                    return null;
                }

                var collection = name[..match.Index];
                current = current.ElementMetadata;
                if (current is not null && IsKeyValuePair(current.ModelType))
                {
                    var next = position + 1 < matches.Count ? matches[position + 1].Groups["name"] : null;
                    var namesKeyOrValue = next is { Success: true }
                                          && (string.Equals(next.Value, "Key", StringComparison.OrdinalIgnoreCase)
                                              || string.Equals(next.Value, "Value", StringComparison.OrdinalIgnoreCase));
                    if (namesKeyOrValue)
                    {
                        AddIndex(indexesByCollection, collection, match.Groups["index"].Value);
                    }
                    else
                    {
                        current = current.Properties.Single(property => property.PropertyName == "Value");
                    }
                }
                else
                {
                    AddIndex(indexesByCollection, collection, match.Groups["index"].Value);
                }

                continue;
            }

            var segment = match.Groups["name"].Value;
            if (current.IsEnumerableType && string.Equals(segment, "index", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            current = current.Properties.FirstOrDefault(property =>
                string.Equals(property.BinderModelName ?? property.PropertyName, segment, StringComparison.OrdinalIgnoreCase));
        }

        return current;
    }

    private static void AddIndex(Dictionary<string, List<string>> indexesByCollection, string collection, string index)
    {
        if (!indexesByCollection.TryGetValue(collection, out var indexes))
        {
            indexes = [];
            indexesByCollection.Add(collection, indexes);
        }

        indexes.Add(index);
    }

    private static bool IsKeyValuePair(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>);
}
