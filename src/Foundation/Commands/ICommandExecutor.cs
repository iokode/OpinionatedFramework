using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Facades;

namespace IOKode.OpinionatedFramework.Commands;

/// <summary>
/// Defines the contract for a command executor, responsible for invoking commands.
/// </summary>
/// <remarks>
/// Invoking a command runs the middleware pipeline; once it has run, the executor validates the command with
/// <see cref="CommandValidation"/>, and only when its data is valid does it prepare and execute it.
/// </remarks>
[AddToFacade("Command")]
public interface ICommandExecutor
{
    /// <summary>
    /// Invokes the specified command asynchronously.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command to be executed.</typeparam>
    /// <param name="command">The command instance to be executed.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to signal the request to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="InvalidInputException">The data of the command is not valid; the command is neither prepared nor executed.</exception>
    public Task InvokeAsync<TCommand>(TCommand command, CancellationToken cancellationToken) where TCommand : Command;

    /// <summary>
    /// Invokes the specified command asynchronously and returns the result.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command to be executed.</typeparam>
    /// <typeparam name="TResult">The type of the result returned by the command.</typeparam>
    /// <param name="command">The command instance to be executed.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to signal the request to cancel the operation.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the result of the command execution.</returns>
    /// <exception cref="InvalidInputException">The data of the command is not valid; the command is neither prepared nor executed.</exception>
    public Task<TResult> InvokeAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken) where TCommand : Command<TResult>;
}