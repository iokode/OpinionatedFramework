using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;

namespace IOKode.OpinionatedFramework.Contracts;

/// <summary>
/// Defines the contract for a command executor, responsible for invoking commands.
/// </summary>
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
    public Task InvokeAsync<TCommand>(TCommand command, CancellationToken cancellationToken) where TCommand : Command;

    /// <summary>
    /// Invokes the specified command asynchronously and returns the result.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command to be executed.</typeparam>
    /// <typeparam name="TResult">The type of the result returned by the command.</typeparam>
    /// <param name="command">The command instance to be executed.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to signal the request to cancel the operation.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the result of the command execution.</returns>
    public Task<TResult> InvokeAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken) where TCommand : Command<TResult>;
}