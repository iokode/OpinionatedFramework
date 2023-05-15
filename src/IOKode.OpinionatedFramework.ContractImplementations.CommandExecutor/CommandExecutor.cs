using System.Reflection;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.ContractImplementations.CommandExecutor;

public class CommandExecutor : ICommandExecutor
{
    private void _setScope(IServiceScope scope)
    {
        var scopedServiceProviderField =
            typeof(Locator).GetField("_scopedServiceProvider", BindingFlags.NonPublic | BindingFlags.Static)!;

        ((AsyncLocal<IServiceProvider?>)scopedServiceProviderField.GetValue(null)!).Value = scope.ServiceProvider;
    }

    private MethodInfo _getExecuteMethod<TCommand>()
    {
        var executeMethod = typeof(TCommand).GetMethod("ExecuteAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        return executeMethod;
    }

    public async Task InvokeAsync<TCommand>(TCommand command, CancellationToken cancellationToken)
        where TCommand : Command
    {
        using var scope = Locator.ServiceProvider.CreateScope();
        _setScope(scope);

        await (Task)_getExecuteMethod<TCommand>().Invoke(command, new object?[] { cancellationToken })!;
    }

    public async Task<TResult> InvokeAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken)
        where TCommand : Command<TResult>
    {
        using var scope = Locator.ServiceProvider.CreateScope();
        _setScope(scope);

        var commandResult = await (Task<TResult>)_getExecuteMethod<TCommand>().Invoke(command, new object?[] { cancellationToken })!;
        return commandResult;
    }
}