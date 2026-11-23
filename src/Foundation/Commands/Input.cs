using System;
using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Commands;

/// <summary>
/// A model of structured data a command receives, such as the data of a passenger or of a seat.
/// </summary>
/// <remarks>
/// An input is plain data: strings, numbers, booleans, other inputs, collections and dictionaries of inputs,
/// <see cref="Updatable{T}"/>, and values whose reading is only a matter of form, such as a typed identifier or a code
/// with a format. A value object whose construction enforces business rules does not belong in an input. An input does
/// not hold the keys of a resource: they are properties of the command.
/// </remarks>
public abstract class Input
{
    /// <summary>
    /// Validates whether the data of this input is acceptable, adding to <paramref name="errors"/> what is wrong with it.
    /// Nothing added means it is.
    /// </summary>
    /// <param name="errors">
    /// The errors of this input, empty when it is received, with paths relative to this input.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <remarks>
    /// By default it calls <see cref="ValidateNestedInputsAsync"/>. An override replaces that behavior: one that adds
    /// rules calls <see cref="ValidateNestedInputsAsync"/> to keep validating the inputs of this input, and one that
    /// does not call it validates none of them.
    /// </remarks>
    public virtual Task ValidateAsync(ValidationErrors errors, CancellationToken cancellationToken)
    {
        return ValidateNestedInputsAsync(errors, cancellationToken);
    }

    /// <summary>
    /// Goes through the public properties of this input and validates every <see cref="Input"/> they hold, each with a
    /// <see cref="ValidationErrors"/> of its own whose errors are added to <paramref name="errors"/> prefixing the name of
    /// the property to their path; every element of a collection of inputs, with its index in the path;
    /// every value of a dictionary of inputs, with its key in the path; going into nested collections and dictionaries;
    /// and whatever of them is inside an <see cref="Updatable{T}"/>, only when it is to be updated.
    /// </summary>
    /// <remarks>
    /// A <see langword="null"/> one is an error at its path, with the message "A value is required.", when its declared
    /// nullability does not allow <see langword="null"/>: a property, an element type, a value type of a dictionary or
    /// the type argument of an <see cref="Updatable{T}"/> declared without <c>?</c> in code with nullable annotations. It
    /// is skipped when its declared nullability allows <see langword="null"/>, and when the code that declares it has no
    /// nullable annotations, since nothing can be told then.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="errors"/> is <see langword="null"/>.</exception>
    protected Task ValidateNestedInputsAsync(ValidationErrors errors, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(errors);
        return InputsValidation.ValidateNestedInputsAsync(this, errors, cancellationToken);
    }
}
