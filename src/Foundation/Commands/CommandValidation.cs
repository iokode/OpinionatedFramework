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
    /// Validates <paramref name="command"/> and throws when its data is not valid.
    /// </summary>
    /// <exception cref="InvalidInputException">The validation of the command found errors; it carries all of them.</exception>
    /// <exception cref="InvalidValidationResultException">The validation of the command returned <see langword="null"/>.</exception>
    public static async Task ThrowIfInvalidAsync(Command command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ThrowIfInvalid(command, await command.ValidateAsync(cancellationToken));
    }

    /// <inheritdoc cref="ThrowIfInvalidAsync(Command, CancellationToken)"/>
    public static async Task ThrowIfInvalidAsync<TResult>(Command<TResult> command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ThrowIfInvalid(command, await command.ValidateAsync(cancellationToken));
    }

    private static void ThrowIfInvalid(object command, ValidationErrors? errors)
    {
        if (errors is null)
        {
            throw new InvalidValidationResultException(command.GetType());
        }

        if (errors.Count > 0)
        {
            throw new InvalidInputException(command, errors);
        }
    }
}
