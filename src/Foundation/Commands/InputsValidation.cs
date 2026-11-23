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
    private const string ValueRequiredMessage = "A value is required.";

    private static readonly ConcurrentDictionary<Type, NestedProperty[]> propertiesByType = new();
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
    /// <see langword="null"/> one is an error at its path when its declared nullability does not allow
    /// <see langword="null"/>, and is skipped when it allows it or when the declaring code has no nullable annotations.
    /// Each input is validated with a <see cref="ValidationErrors"/> of its own, and its errors are added to
    /// <paramref name="errors"/> under the path of the input.
    /// </summary>
    public static async Task ValidateNestedInputsAsync(object owner, ValidationErrors errors, CancellationToken cancellationToken)
    {
        foreach (var property in GetProperties(owner.GetType()))
        {
            var value = property.Info.GetValue(owner, BindingFlags.DoNotWrapExceptions, null, null, null);
            var position = property.Position;
            if (value is IUpdatable updatable)
            {
                if (!updatable.ShouldUpdate)
                {
                    continue;
                }

                value = updatable.Value;
                position = property.UpdatableValuePosition;
            }

            var propertyErrors = await ValidateValueAsync(value, position, cancellationToken);
            if (propertyErrors is not null)
            {
                errors.AddNested(new PropertyPathSegment(property.Info.Name), propertyErrors);
            }
        }
    }

    /// <summary>
    /// Validates a value when it is an input, or a collection or dictionary whose elements are inputs or, in turn,
    /// collections or dictionaries of them, into a new <see cref="ValidationErrors"/> with paths relative to the value.
    /// A <see langword="null"/> value is an error when <paramref name="position"/> does not allow it. Returns
    /// <see langword="null"/> when there is nothing to validate.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="position">
    /// The declared nullability of the place that holds <paramref name="value"/>, or <see langword="null"/> when it is
    /// not known.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    private static async Task<ValidationErrors?> ValidateValueAsync(object? value, Position? position, CancellationToken cancellationToken)
    {
        if (value is null)
        {
            if (position is not { RejectsNull: true })
            {
                return null;
            }

            var nullErrors = new ValidationErrors();
            nullErrors.AddForObject(ValueRequiredMessage);
            return nullErrors;
        }

        switch (GetKind(value.GetType()))
        {
            case ValueKind.Input:
            {
                var inputErrors = new ValidationErrors();
                await ((Input) value).ValidateAsync(inputErrors, cancellationToken);
                return inputErrors;
            }
            case ValueKind.Dictionary:
            {
                var errors = new ValidationErrors();
                foreach (var entry in (IEnumerable) value)
                {
                    var (key, element) = ReadKeyValuePair(value.GetType(), entry!);
                    var elementErrors = await ValidateValueAsync(element, position?.Element, cancellationToken);
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
                    var elementErrors = await ValidateValueAsync(element, position?.Element, cancellationToken);
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

    private static NestedProperty[] GetProperties(Type type)
    {
        return propertiesByType.GetOrAdd(type, static type =>
        {
            var nullabilityContext = new NullabilityInfoContext();
            return type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.GetMethod is { IsPublic: true } && property.GetIndexParameters().Length == 0)
                .Select(property => DescribeProperty(property, nullabilityContext.Create(property)))
                .ToArray();
        });
    }

    private static NestedProperty DescribeProperty(PropertyInfo property, NullabilityInfo nullability)
    {
        var position = Describe(nullability);
        var updatableValuePosition = property.PropertyType.IsGenericType
                                     && property.PropertyType.GetGenericTypeDefinition() == typeof(Updatable<>)
            ? Describe(nullability.GenericTypeArguments[0])
            : null;

        return new NestedProperty(property, position, updatableValuePosition);
    }

    /// <summary>
    /// Describes the declared nullability of a place that can hold an input, or a collection or dictionary of them, and
    /// of the elements or values it holds. Returns <see langword="null"/> when the declared type is none of them.
    /// </summary>
    private static Position? Describe(NullabilityInfo nullability)
    {
        var kind = GetKind(nullability.Type);
        if (kind == ValueKind.None)
        {
            return null;
        }

        var element = kind switch
        {
            ValueKind.Collection => FindElement(nullability, ValueKind.Collection),
            ValueKind.Dictionary => FindElement(nullability, ValueKind.Dictionary),
            _ => null,
        };

        return new Position(
            nullability.ReadState == NullabilityState.NotNull,
            element is null ? null : Describe(element));
    }

    /// <summary>
    /// Finds the nullability of the elements of a collection, or of the values of a dictionary, from the type arguments
    /// of its declared type. Returns <see langword="null"/> when they cannot be told: the declared type takes them from
    /// none of its type arguments, or from more than one way.
    /// </summary>
    private static NullabilityInfo? FindElement(NullabilityInfo nullability, ValueKind kind)
    {
        var type = nullability.Type;
        if (type.IsArray)
        {
            return nullability.ElementType is { } arrayElement ? SelectElement(arrayElement, kind) : null;
        }

        if (!type.IsGenericType)
        {
            return null;
        }

        var definition = type.GetGenericTypeDefinition();
        var candidates = definition.GetInterfaces()
            .Prepend(definition)
            .Where(candidate => candidate.IsGenericType && candidate.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            .Select(enumerable => ResolveElement(enumerable.GetGenericArguments()[0], nullability, kind))
            .Where(element => element is not null && GetKind(element.Type) != ValueKind.None)
            .ToArray();

        return candidates.Length == 1 ? candidates[0] : null;
    }

    /// <summary>
    /// Resolves the element type of an <see cref="IEnumerable{T}"/> implemented by a generic type definition, expressed
    /// in its generic parameters, to the nullability of the type argument it stands for.
    /// </summary>
    private static NullabilityInfo? ResolveElement(Type openElement, NullabilityInfo nullability, ValueKind kind)
    {
        if (openElement.IsGenericParameter)
        {
            return SelectElement(nullability.GenericTypeArguments[openElement.GenericParameterPosition], kind);
        }

        if (kind == ValueKind.Dictionary
            && openElement.IsGenericType
            && openElement.GetGenericTypeDefinition() == typeof(KeyValuePair<,>)
            && openElement.GetGenericArguments()[1] is { IsGenericParameter: true } openValue)
        {
            return nullability.GenericTypeArguments[openValue.GenericParameterPosition];
        }

        return null;
    }

    /// <summary>
    /// Returns the nullability of an element of a collection, or of the value of the <see cref="KeyValuePair{TKey,TValue}"/>
    /// that is an element of a dictionary.
    /// </summary>
    private static NullabilityInfo? SelectElement(NullabilityInfo element, ValueKind kind)
    {
        if (kind != ValueKind.Dictionary)
        {
            return element;
        }

        return element.Type.IsGenericType && element.Type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>)
            ? element.GenericTypeArguments[1]
            : null;
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

    private static (object Key, object? Value) ReadKeyValuePair(Type dictionaryType, object entry)
    {
        var entryType = entry.GetType();
        var key = entryType.GetProperty(nameof(KeyValuePair<object, object>.Key))!.GetValue(entry)
                  ?? throw new NullDictionaryKeyException(dictionaryType);
        var value = entryType.GetProperty(nameof(KeyValuePair<object, object>.Value))!.GetValue(entry);
        return (key, value);
    }

    /// <summary>
    /// A public property of a command or an input, with the declared nullability of what it holds and, when it is an
    /// <see cref="Updatable{T}"/>, of the value inside it.
    /// </summary>
    private sealed record NestedProperty(PropertyInfo Info, Position? Position, Position? UpdatableValuePosition);

    /// <summary>
    /// The declared nullability of a place that can hold an input, or a collection or dictionary of them.
    /// </summary>
    /// <param name="RejectsNull">Whether the declared type does not allow <see langword="null"/>.</param>
    /// <param name="Element">
    /// The declared nullability of the elements or values it holds, or <see langword="null"/> when it is not known.
    /// </param>
    private sealed record Position(bool RejectsNull, Position? Element);
}
