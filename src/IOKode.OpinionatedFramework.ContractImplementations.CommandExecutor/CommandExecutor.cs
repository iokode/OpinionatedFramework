using System.Reflection;
using IOKode.OpinionatedFramework.Contracts;
using IOKode.OpinionatedFramework.Foundation;
using IOKode.OpinionatedFramework.Foundation.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.ContractImplementations.CommandExecutor;

public class CommandExecutor : ICommandExecutor
{
    public Task InvokeAsync<TCommand>(TCommand command) where TCommand : Command
    {
        return InvokeCommandAsync(command, async cmd =>
        {
            await cmd.ExecuteAsync();
            return Task.CompletedTask;
        });
    }

    public Task<TResult> InvokeAsync<TCommand, TResult>(TCommand command) where TCommand : Command<TResult>
    {
        return InvokeCommandAsync(command, async cmd => await cmd.ExecuteAsync());
    }

    private async Task<TResult> InvokeCommandAsync<TCommand, TResult>(TCommand command,
        Func<TCommand, Task<TResult>> commandFunc)
    {
        using var scope = Locator.ServiceProvider!.CreateScope();

        var originalExecutionContext = ExecutionContext.Capture();
        var newExecutionContext = originalExecutionContext!.CreateCopy();

        TResult? result = default;

        ExecutionContext.Run(newExecutionContext, async _ =>
        {
            var scopedServiceProviderField =
                typeof(Locator).GetField("_scopedServiceProvider", BindingFlags.NonPublic | BindingFlags.Static)!;

            scopedServiceProviderField.SetValue(null, scope.ServiceProvider);

            try
            {
                result = await commandFunc(command);
            }
            finally
            {
                // Reset scoped ServiceProvider for the current ExecutionContext
                scopedServiceProviderField.SetValue(null, null);
            }
        }, null);

        return result!;
    }
}