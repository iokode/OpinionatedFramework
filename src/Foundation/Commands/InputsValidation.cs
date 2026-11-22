using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Commands;

/// <summary>
/// Validates the inputs held by the public properties of a command or an input.
/// </summary>
internal static class InputsValidation
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> propertiesByType = new();
    private static readonly ConcurrentDictionary<Type, ValueKind> kindsByType = new();

    private enum ValueKind
    {
        None,
        Input,
        Collection,
        Dictionary,
    }

    /// <summary>
    /// Goes through the public properties of <paramref name="owner"/> and validates every <see cref="Input"/>, every
    /// element of a collection of inputs, every value of a dictionary of inputs, going into nested collections and
    /// dictionaries, and whatever of them is inside an <see cref="Updatable{T}"/> that is to be updated. A
    /// <see langword="null"/> one has nothing to validate and is skipped.
    /// </summary>
    public static async Task<ValidationErrors> ValidateInputsAsync(object owner, CancellationToken cancellationToken)
    {
        var errors = new ValidationErrors();
        foreach (var property in GetProperties(owner.GetType()))
        {
            var value = property.GetValue(owner, BindingFlags.DoNotWrapExceptions, null, null, null);
            if (value is IUpdatable updatable)
            {
                if (!updatable.ShouldUpdate)
                {
                    continue;
                }

                value = updatable.Value;
            }

            var propertyErrors = await ValidateValueAsync(value, cancellationToken);
            if (propertyErrors is not null)
            {
                errors.AddNested(new PropertyPathSegment(property.Name), propertyErrors);
            }
        }

        return errors;
    }

    /// <summary>
    /// Validates a value when it is an input, or a collection or dictionary whose elements are inputs or, in turn,
    /// collections or dictionaries of them. Returns <see langword="null"/> when it is none of them.
    /// </summary>
    private static async Task<ValidationErrors?> ValidateValueAsync(object? value, CancellationToken cancellationToken)
    {
        if (value is null)
        {
            return null;
        }

        switch (GetKind(value.GetType()))
        {
            case ValueKind.Input:
            {
                var input = (Input) value;
                return await input.ValidateAsync(cancellationToken)
                       ?? throw new InvalidOperationException($"The validation of the input '{input.GetType()}' returned null instead of its errors.");
            }
            case ValueKind.Dictionary:
            {
                var errors = new ValidationErrors();
                foreach (var entry in (IEnumerable) value)
                {
                    var (key, element) = ReadKeyValuePair(entry!);
                    var elementErrors = await ValidateValueAsync(element, cancellationToken);
                    if (elementErrors is not null)
                    {
                        errors.AddNested(new KeyPathSegment(key), elementErrors);
                    }
                }

                return errors;
            }
            case ValueKind.Collection:
            {
                var errors = new ValidationErrors();
                var index = 0;
                foreach (var element in (IEnumerable) value)
                {
                    var elementErrors = await ValidateValueAsync(element, cancellationToken);
                    if (elementErrors is not null)
                    {
                        errors.AddNested(new IndexPathSegment(index), elementErrors);
                    }

                    index++;
                }

                return errors;
            }
            default:
                return null;
        }
    }

    private static PropertyInfo[] GetProperties(Type type)
    {
        return propertiesByType.GetOrAdd(type, static type => type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.GetMethod is { IsPublic: true } && property.GetIndexParameters().Length == 0)
            .ToArray());
    }

    private static ValueKind GetKind(Type type)
    {
        return kindsByType.GetOrAdd(type, static type => Classify(type, []));
    }

    /// <summary>
    /// Classifies a type by its declared element types, going into nested collections and dictionaries.
    /// <paramref name="visiting"/> holds the types being classified, so that a type enumerating itself ends.
    /// </summary>
    private static ValueKind Classify(Type type, HashSet<Type> visiting)
    {
        if (typeof(Input).IsAssignableFrom(type))
        {
            return ValueKind.Input;
        }

        if (!visiting.Add(type))
        {
            return ValueKind.None;
        }

        try
        {
            var elementTypes = type.GetInterfaces()
                .Prepend(type)
                .Where(candidate => candidate.IsGenericType && candidate.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .Select(enumerable => enumerable.GetGenericArguments()[0])
                .ToArray();

            if (elementTypes.Any(elementType =>
                    elementType.IsGenericType
                    && elementType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>)
                    && Classify(elementType.GetGenericArguments()[1], visiting) != ValueKind.None))
            {
                return ValueKind.Dictionary;
            }

            if (elementTypes.Any(elementType => Classify(elementType, visiting) != ValueKind.None))
            {
                return ValueKind.Collection;
            }

            return ValueKind.None;
        }
        finally
        {
            visiting.Remove(type);
        }
    }

    private static (object Key, object? Value) ReadKeyValuePair(object entry)
    {
        var entryType = entry.GetType();
        var key = entryType.GetProperty(nameof(KeyValuePair<object, object>.Key))!.GetValue(entry)!;
        var value = entryType.GetProperty(nameof(KeyValuePair<object, object>.Value))!.GetValue(entry);
        return (key, value);
    }
}
