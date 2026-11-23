using System;
using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Commands;

/// <summary>
/// The validation step of the execution of a command, which every implementation of <see cref="ICommandExecutor"/>
/// runs once the middleware pipeline has run and before preparing the command, so that all of them do the same.
/// </summary>
public static class CommandValidation
{
    /// <summary>
    /// Validates <paramref name="command"/> with an empty <see cref="ValidationErrors"/> and throws when its data is not
    /// valid.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="command"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidInputException">The validation of the command found errors; it carries all of them.</exception>
    public static async Task ThrowIfInvalidAsync(Command command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var errors = new ValidationErrors();
        await command.ValidateAsync(errors, cancellationToken);
        ThrowIfAny(command, errors);
    }

    /// <inheritdoc cref="ThrowIfInvalidAsync(Command, CancellationToken)"/>
    public static async Task ThrowIfInvalidAsync<TResult>(Command<TResult> command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var errors = new ValidationErrors();
        await command.ValidateAsync(errors, cancellationToken);
        ThrowIfAny(command, errors);
    }

    private static void ThrowIfAny(object command, ValidationErrors errors)
    {
        if (errors.Count > 0)
        {
            throw new InvalidInputException(command, errors);
        }
    }
}
