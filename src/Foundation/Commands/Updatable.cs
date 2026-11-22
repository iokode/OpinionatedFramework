namespace IOKode.OpinionatedFramework.Commands;

/// <summary>
/// A value that is changed only when it is given. A command that updates part of a resource receives each of the values
/// it may change as an <see cref="Updatable{T}"/>, which says whether that value is to be changed and to what.
/// </summary>
/// <remarks>
/// The default value is not to be changed. Validation goes inside it only when <see cref="ShouldUpdate"/> is
/// <see langword="true"/>.
/// </remarks>
public readonly struct Updatable<T> : IUpdatable
{
    /// <summary>
    /// The value to change to. Meaningful only when <see cref="ShouldUpdate"/> is <see langword="true"/>.
    /// </summary>
    public T Value { get; init; }

    /// <summary>
    /// Whether the value is to be changed.
    /// </summary>
    public bool ShouldUpdate { get; init; }

    /// <summary>
    /// A value that is to be changed to <paramref name="value"/>.
    /// </summary>
    public Updatable(T value)
    {
        Value = value;
        ShouldUpdate = true;
    }

    public static implicit operator Updatable<T>(T value) => new(value);

    object? IUpdatable.Value => Value;
}

/// <summary>
/// Gives validation access to an <see cref="Updatable{T}"/> whatever its type argument.
/// </summary>
internal interface IUpdatable
{
    bool ShouldUpdate { get; }
    object? Value { get; }
}
