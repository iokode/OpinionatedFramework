using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Commands;

/// <summary>
/// A use case that returns no result. It receives its data as public properties.
/// </summary>
public abstract class Command
{
    /// <summary>
    /// Validates whether the data of this command is acceptable for its use case. An empty result means it is.
    /// </summary>
    /// <remarks>
    /// The executor calls it once the middleware pipeline has run and before <see cref="PrepareAsync"/>. By default it
    /// calls <see cref="ValidateInputsAsync"/>. An override replaces that behavior: one that adds rules calls
    /// <see cref="ValidateInputsAsync"/> to keep validating the inputs of this command, and one that does not call it
    /// validates none of them.
    /// </remarks>
    public virtual Task<ValidationErrors> ValidateAsync(CancellationToken cancellationToken)
    {
        return ValidateInputsAsync(cancellationToken);
    }

    /// <summary>
    /// Goes through the public properties of this command and validates every <see cref="Input"/>, prefixing the name
    /// of the property to the path of its errors; every element of a collection of inputs, with its index in the path;
    /// every value of a dictionary of inputs, with its key in the path; going into nested collections and dictionaries;
    /// and whatever of them is inside an <see cref="Updatable{T}"/>, only when it is to be updated. A
    /// <see langword="null"/> one has nothing to validate and is skipped.
    /// </summary>
    protected Task<ValidationErrors> ValidateInputsAsync(CancellationToken cancellationToken)
    {
        return InputsValidation.ValidateInputsAsync(this, cancellationToken);
    }

    protected virtual Task PrepareAsync(ICommandExecutionContext executionContext)
    {
        return Task.CompletedTask;
    }

    protected abstract Task ExecuteAsync(ICommandExecutionContext executionContext);
}

/// <summary>
/// A use case that returns a result. It receives its data as public properties.
/// </summary>
public abstract class Command<TResult>
{
    /// <inheritdoc cref="Command.ValidateAsync"/>
    public virtual Task<ValidationErrors> ValidateAsync(CancellationToken cancellationToken)
    {
        return ValidateInputsAsync(cancellationToken);
    }

    /// <inheritdoc cref="Command.ValidateInputsAsync"/>
    protected Task<ValidationErrors> ValidateInputsAsync(CancellationToken cancellationToken)
    {
        return InputsValidation.ValidateInputsAsync(this, cancellationToken);
    }

    protected virtual Task PrepareAsync(ICommandExecutionContext executionContext)
    {
        return Task.CompletedTask;
    }

    protected abstract Task<TResult> ExecuteAsync(ICommandExecutionContext executionContext);
}
