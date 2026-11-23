using System;
using System.Linq;

namespace IOKode.OpinionatedFramework.Commands;

/// <summary>
/// The exception that is thrown when the data a command receives is not valid. The command is neither prepared nor
/// executed.
/// </summary>
public class InvalidInputException : Exception
{
    /// <param name="command">The command whose data is not valid.</param>
    /// <param name="errors">All the errors found while validating the data of <paramref name="command"/>.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="command"/> or <paramref name="errors"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException"><paramref name="errors"/> is empty.</exception>
    public InvalidInputException(object command, ValidationErrors errors) : base(BuildMessage(command, errors))
    {
        Command = command;
        Errors = errors;
    }

    /// <summary>
    /// The command whose data is not valid, which tells it apart from another command invoked while running it.
    /// </summary>
    public object Command { get; }

    /// <summary>
    /// All the errors found while validating the data.
    /// </summary>
    public ValidationErrors Errors { get; }

    private static string BuildMessage(object command, ValidationErrors errors)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(errors);
        if (errors.Count == 0)
        {
            throw new ArgumentException(
                $"The errors of the invalid input of the command '{command.GetType()}' are empty; an invalid input has at least one validation error.",
                nameof(errors));
        }

        return $"The input data of the command '{command.GetType()}' is not valid:" + string.Concat(errors.Select(error => $"{Environment.NewLine}- {error}"));
    }
}
