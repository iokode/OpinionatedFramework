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
    /// Validates whether the data of this input is acceptable. An empty result means it is.
    /// </summary>
    /// <remarks>
    /// By default it calls <see cref="ValidateInputsAsync"/>. An override replaces that behavior: one that adds rules
    /// calls <see cref="ValidateInputsAsync"/> to keep validating the inputs of this input, and one that does not call
    /// it validates none of them.
    /// </remarks>
    public virtual Task<ValidationErrors> ValidateAsync(CancellationToken cancellationToken)
    {
        return ValidateInputsAsync(cancellationToken);
    }

    /// <summary>
    /// Goes through the public properties of this input and validates every <see cref="Input"/>, prefixing the name of
    /// the property to the path of its errors; every element of a collection of inputs, with its index in the path;
    /// every value of a dictionary of inputs, with its key in the path; going into nested collections and dictionaries;
    /// and whatever of them is inside an <see cref="Updatable{T}"/>, only when it is to be updated. A
    /// <see langword="null"/> one has nothing to validate and is skipped.
    /// </summary>
    protected Task<ValidationErrors> ValidateInputsAsync(CancellationToken cancellationToken)
    {
        return InputsValidation.ValidateInputsAsync(this, cancellationToken);
    }
}
