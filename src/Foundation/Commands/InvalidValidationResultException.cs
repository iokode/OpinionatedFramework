using System;

namespace IOKode.OpinionatedFramework.Commands;

/// <summary>
/// The exception that is thrown when the validation of a command or an input returns <see langword="null"/> instead of
/// its errors, which breaks the contract of <c>ValidateAsync</c>: valid data is an empty
/// <see cref="ValidationErrors"/>.
/// </summary>
public class InvalidValidationResultException : Exception
{
    /// <param name="validatedType">The type of the command or input whose validation returned <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="validatedType"/> is <see langword="null"/>.</exception>
    public InvalidValidationResultException(Type validatedType) : base(BuildMessage(validatedType))
    {
        ValidatedType = validatedType;
    }

    /// <summary>
    /// The type of the command or input whose validation returned <see langword="null"/>.
    /// </summary>
    public Type ValidatedType { get; }

    private static string BuildMessage(Type validatedType)
    {
        ArgumentNullException.ThrowIfNull(validatedType);
        return $"The validation of '{validatedType}' returned null instead of its errors; valid data is an empty {nameof(ValidationErrors)}.";
    }
}
