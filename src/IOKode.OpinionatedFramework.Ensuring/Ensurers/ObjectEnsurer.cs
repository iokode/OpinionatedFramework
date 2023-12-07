using System;

namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

[Ensurer]
public static class ObjectEnsurer
{
    /// <summary>
    /// Determines whether the specified object is null.
    /// </summary>
    /// <param name="value">The object to check for null.</param>
    /// <returns><c>true</c> if the specified object is null; otherwise, <c>false</c>.</returns>
    public static bool IsNull(object? value)
    {
        return value == null;
    }

    /// <summary>
    /// Determines whether the specified object is not null.
    /// </summary>
    /// <param name="value">The object to check for null.</param>
    /// <returns><c>true</c> if the specified object is not null; otherwise, <c>false</c>.</returns>
    public static bool NotNull(object? value)
    {
        return value != null;
    }

    /// <summary>
    /// Determines whether the specified object instances are considered equal by comparing their values
    /// using the <see cref="object.Equals(object)"/> method from <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The first object to compare. Must not be null.</param>
    /// <param name="other">The second object to compare.</param>
    /// <returns><c>true</c> if the specified objects are considered equal; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool Equivalent(object value, object other)
    {
        Ensure.ArgumentNotNull(value);
        return value.Equals(other);
    }

    /// <summary>
    /// Determines whether the specified object instances are not considered equal by comparing their values
    /// using the <see cref="object.Equals(object)"/> method from <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The first object to compare. Must not be null.</param>
    /// <param name="other">The second object to compare.</param>
    /// <returns><c>true</c> if the specified objects are not considered equal; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool NotEquivalent(object value, object other)
    {
        Ensure.ArgumentNotNull(value);
        return !value.Equals(other);
    }

    /// <summary>
    /// Determines whether the specified objects are the same instance.
    /// </summary>
    /// <param name="value">The first object to compare.</param>
    /// <param name="other">The second object to compare.</param>
    /// <returns><c>true</c> if the specified objects are the same instance; otherwise, <c>false</c>.</returns>
    public static bool Same(object? value, object? other)
    {
        return ReferenceEquals(value, other);
    }

    /// <summary>
    /// Determines whether the specified objects are not the same instance.
    /// </summary>
    /// <param name="value">The first object to compare.</param>
    /// <param name="other">The second object to compare.</param>
    /// <returns><c>true</c> if the specified objects are not the same instance; otherwise, <c>false</c>.</returns>
    public static bool NotSame(object? value, object? other)
    {
        return !ReferenceEquals(value, other);
    }
}