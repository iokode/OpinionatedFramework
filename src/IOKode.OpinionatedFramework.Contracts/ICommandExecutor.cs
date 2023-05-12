using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Foundation.Commands;

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
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task InvokeAsync<TCommand>(TCommand command) where TCommand : Command;

    /// <summary>
    /// Invokes the specified command asynchronously and returns the result.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command to be executed.</typeparam>
    /// <typeparam name="TResult">The type of the result returned by the command.</typeparam>
    /// <param name="command">The command instance to be executed.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the result of the command execution.</returns>
    public Task<TResult> InvokeAsync<TCommand, TResult>(TCommand command) where TCommand : Command<TResult>;
}