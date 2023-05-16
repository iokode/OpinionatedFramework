using System.Reflection;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.ContractImplementations.CommandExecutor;

public class CommandExecutor : ICommandExecutor
{
    private readonly ICommandMiddleware[] _middlewares;

    public CommandExecutor(ICommandMiddleware[] middlewares)
    {
        _middlewares = middlewares;
    }

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

    private async Task _invokeMiddlewarePipeline<TCommand>(TCommand command, CommandContext context, int index,
        CancellationToken cancellationToken)
    {
        if (index >= _middlewares.Length)
        {
            await (Task)_getExecuteMethod<TCommand>().Invoke(command, new object?[] { cancellationToken })!;
            context.IsExecuted = true;
            return;
        }

        var middleware = _middlewares[index];
        await middleware.ExecuteAsync(context,
            ctx => _invokeMiddlewarePipeline(command, ctx, index + 1, cancellationToken));
    }

    private async Task<TResult> _invokeMiddlewarePipeline<TCommand, TResult>(TCommand command, CommandContext context,
        int index, CancellationToken cancellationToken)
        where TCommand : Command<TResult>
    {
        if (index >= _middlewares.Length)
        {
            var commandResult = await 
                (Task<TResult>)_getExecuteMethod<TCommand>().Invoke(command, new object?[] { cancellationToken })!;

            context.IsExecuted = true;
            context.HasResult = true;
            context.Result = commandResult;
            
            return commandResult;
        }

        var middleware = _middlewares[index];
        await middleware.ExecuteAsync(context,
            ctx => _invokeMiddlewarePipeline<TCommand, TResult>(command, ctx, index + 1, cancellationToken));
        return default!;
    }

    private CommandContext _createContext(Type commandType, CancellationToken cancellationToken)
    {
        var context = new CommandContext
        {
            CommandType = commandType,
            CancellationToken = cancellationToken,
            HasResult = false,
            Result = null,
            IsExecuted = false
        };

        return context;
    }

    public async Task InvokeAsync<TCommand>(TCommand command, CancellationToken cancellationToken)
        where TCommand : Command
    {
        using var scope = Locator.ServiceProvider.CreateScope();
        _setScope(scope);

        var context = _createContext(command.GetType(), cancellationToken);
        await _invokeMiddlewarePipeline(command, context, 0, cancellationToken);
    }

    public async Task<TResult> InvokeAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken)
        where TCommand : Command<TResult>
    {
        using var scope = Locator.ServiceProvider.CreateScope();
        _setScope(scope);

        var context = _createContext(command.GetType(), cancellationToken);
        return await _invokeMiddlewarePipeline<TCommand, TResult>(command, context, 0, cancellationToken);
    }
}