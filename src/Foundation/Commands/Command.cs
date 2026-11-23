using System;
using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Commands;

/// <summary>
/// A use case that returns no result. It receives its data as public properties.
/// </summary>
public abstract class Command
{
    /// <summary>
    /// Validates whether the data of this command is acceptable for its use case, adding to <paramref name="errors"/>
    /// what is wrong with it. Nothing added means it is.
    /// </summary>
    /// <param name="errors">The errors of this command, empty when it is received.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <remarks>
    /// The executor calls it once the middleware pipeline has run and before <see cref="PrepareAsync"/>. By default it
    /// calls <see cref="ValidateNestedInputsAsync"/>. An override replaces that behavior: one that adds rules calls
    /// <see cref="ValidateNestedInputsAsync"/> to keep validating the inputs of this command, and one that does not call
    /// it validates none of them.
    /// </remarks>
    public virtual Task ValidateAsync(ValidationErrors errors, CancellationToken cancellationToken)
    {
        return ValidateNestedInputsAsync(errors, cancellationToken);
    }

    /// <summary>
    /// Goes through the public properties of this command and validates every <see cref="Input"/> they hold, each with
    /// a <see cref="ValidationErrors"/> of its own whose errors are added to <paramref name="errors"/> prefixing the name
    /// of the property to their path; every element of a collection of inputs, with its index in the path;
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
    public virtual Task ValidateAsync(ValidationErrors errors, CancellationToken cancellationToken)
    {
        return ValidateNestedInputsAsync(errors, cancellationToken);
    }

    /// <inheritdoc cref="Command.ValidateNestedInputsAsync"/>
    protected Task ValidateNestedInputsAsync(ValidationErrors errors, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(errors);
        return InputsValidation.ValidateNestedInputsAsync(this, errors, cancellationToken);
    }

    protected virtual Task PrepareAsync(ICommandExecutionContext executionContext)
    {
        return Task.CompletedTask;
    }

    protected abstract Task<TResult> ExecuteAsync(ICommandExecutionContext executionContext);
}
