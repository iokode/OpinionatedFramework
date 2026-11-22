using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace IOKode.OpinionatedFramework.Commands;

/// <summary>
/// The path of the data a validation error is about, as a sequence of segments from the command or input that was
/// validated: a property, an index of a collection or a key of a dictionary.
/// </summary>
/// <remarks>
/// Its text form (<c>Passenger.Document.Number</c>, <c>Seats[2].Row</c>, <c>SeatsByCode["a.b"].Row</c>) exists only to
/// show it. Whoever consumes a path goes through its <see cref="Segments"/> and never parses that text.
/// </remarks>
public sealed class ValidationPath : IEquatable<ValidationPath>
{
    private readonly ValidationPathSegment[] segments;

    /// <summary>
    /// Creates a path from its segments, the first one being the outermost.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="segments"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="segments"/> is empty or contains <see langword="null"/>.</exception>
    public ValidationPath(params IEnumerable<ValidationPathSegment> segments)
    {
        ArgumentNullException.ThrowIfNull(segments);

        this.segments = segments.ToArray();
        if (this.segments.Length == 0)
        {
            throw new ArgumentException("A validation path needs at least one segment.", nameof(segments));
        }

        if (this.segments.Any(segment => segment is null))
        {
            throw new ArgumentException("A validation path cannot contain a null segment.", nameof(segments));
        }
    }

    /// <summary>
    /// The segments of the path, the first one being the outermost.
    /// </summary>
    public IReadOnlyList<ValidationPathSegment> Segments => this.segments;

    /// <summary>
    /// Returns a path with <paramref name="segment"/> before the segments of this one.
    /// </summary>
    internal ValidationPath Prepend(ValidationPathSegment segment)
    {
        return new ValidationPath(this.segments.Prepend(segment));
    }

    public bool Equals(ValidationPath? other)
    {
        return other is not null && this.segments.SequenceEqual(other.segments);
    }

    public override bool Equals(object? obj)
    {
        return obj is ValidationPath other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var segment in this.segments)
        {
            hash.Add(segment);
        }

        return hash.ToHashCode();
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        foreach (var segment in this.segments)
        {
            switch (segment)
            {
                case PropertyPathSegment property:
                    if (builder.Length > 0)
                    {
                        builder.Append('.');
                    }

                    builder.Append(property.Name);
                    break;
                case IndexPathSegment index:
                    builder.Append('[').Append(index.Index.ToString(CultureInfo.InvariantCulture)).Append(']');
                    break;
                case KeyPathSegment key:
                    builder.Append('[').Append(FormatKey(key.Key)).Append(']');
                    break;
                default:
                    throw new InvalidOperationException($"Unknown validation path segment '{segment.GetType()}'.");
            }
        }

        return builder.ToString();
    }

    private static string FormatKey(object key)
    {
        if (key is string text)
        {
            return "\"" + text.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }

        return key is IFormattable formattable
            ? formattable.ToString(null, CultureInfo.InvariantCulture)
            : key.ToString() ?? string.Empty;
    }
}

/// <summary>
/// A segment of a <see cref="ValidationPath"/>.
/// </summary>
public abstract record ValidationPathSegment
{
    private protected ValidationPathSegment()
    {
    }
}

/// <summary>
/// A segment of a <see cref="ValidationPath"/> that is a property, by its name.
/// </summary>
public sealed record PropertyPathSegment : ValidationPathSegment
{
    /// <exception cref="ArgumentException"><paramref name="name"/> is <see langword="null"/>, empty or white space.</exception>
    public PropertyPathSegment(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    /// <summary>
    /// The name of the property.
    /// </summary>
    public string Name { get; }
}

/// <summary>
/// A segment of a <see cref="ValidationPath"/> that is the index of an element of a collection.
/// </summary>
public sealed record IndexPathSegment : ValidationPathSegment
{
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is negative.</exception>
    public IndexPathSegment(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        Index = index;
    }

    /// <summary>
    /// The index of the element.
    /// </summary>
    public int Index { get; }
}

/// <summary>
/// A segment of a <see cref="ValidationPath"/> that is the key of a value of a dictionary.
/// </summary>
public sealed record KeyPathSegment : ValidationPathSegment
{
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
    public KeyPathSegment(object key)
    {
        ArgumentNullException.ThrowIfNull(key);
        Key = key;
    }

    /// <summary>
    /// The key itself, whatever its type.
    /// </summary>
    public object Key { get; }
}
