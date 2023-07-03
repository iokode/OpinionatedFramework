using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.ConfigureApplication;
using Command = IOKode.OpinionatedFramework.Commands.Command;

namespace IOKode.OpinionatedFramework.ContractImplementations.CommandExecutor;

public class CommandExecutor : ICommandExecutor
{
    private readonly CommandMiddleware[] _middlewares;
    private readonly IEnumerable<KeyValuePair<string, object>>? _sharedData;

    public CommandExecutor(CommandMiddleware[] middlewares,
        IEnumerable<KeyValuePair<string, object>>? sharedData = null)
    {
        _middlewares = middlewares;
        _sharedData = sharedData;
    }

    public async Task InvokeAsync<TCommand>(TCommand command, CancellationToken cancellationToken)
        where TCommand : Command
    {
        using (Container.CreateScope())
        {
            var context = SettableCommandContext.Create(command.GetType(), _sharedData, cancellationToken);
            await InvokeMiddlewarePipeline(command, context, 0);
        }
    }

    public async Task<TResult> InvokeAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken)
        where TCommand : Command<TResult>
    {
        using (Container.CreateScope())
        {
            var context = SettableCommandContext.Create(command.GetType(), _sharedData, cancellationToken);
            return await InvokeMiddlewarePipeline<TCommand, TResult>(command, context, 0);
        }
    }

    private static MethodInfo GetExecuteMethod<TCommand>()
    {
        var executeMethod = typeof(TCommand).GetMethod("ExecuteAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        return executeMethod;
    }

    private static MethodInfo GetPrepareMethod<TCommand>()
    {
        var executeMethod = typeof(TCommand).GetMethod("PrepareAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        return executeMethod;
    }

    private async Task PrepareAndExecuteAsync<TCommand>(TCommand command, SettableCommandContext context)
    {
        var prepare = GetPrepareMethod<TCommand>();
        var execute = GetExecuteMethod<TCommand>();
        var parameters = new object?[] { context };

        try
        {
            await (Task)prepare.Invoke(command, parameters)!;
            await (Task)execute.Invoke(command, parameters)!;
        }
        catch (TargetInvocationException ex)
        {
            throw ex.InnerException!;
        }

        context.SetAsExecuted();
    }
    
    private async Task<TResult> PrepareAndExecuteAsync<TCommand, TResult>(TCommand command, SettableCommandContext context)
    {
        var prepare = GetPrepareMethod<TCommand>();
        var execute = GetExecuteMethod<TCommand>();
        var parameters = new object?[] { context };

        await (Task)prepare.Invoke(command, parameters)!;
        var result = await (Task<TResult>)execute.Invoke(command, parameters)!;
        
        context.SetAsExecuted();
        context.SetResult(result);

        return result;
    }

    private async Task InvokeMiddlewarePipeline<TCommand>(TCommand command, SettableCommandContext context, int index)
    {
        if (index >= _middlewares.Length)
        {
            await PrepareAndExecuteAsync(command, context);
            return;
        }

        var middleware = _middlewares[index];
        await middleware.ExecuteAsync(context,
            ctx => InvokeMiddlewarePipeline(command, (SettableCommandContext)ctx, index + 1));
    }

    private async Task<TResult> InvokeMiddlewarePipeline<TCommand, TResult>(TCommand command,
        SettableCommandContext context, int index)
        where TCommand : Command<TResult>
    {
        TResult result = default!;
        if (index >= _middlewares.Length)
        {
            return await PrepareAndExecuteAsync<TCommand, TResult>(command, context);
        }

        var middleware = _middlewares[index];
        await middleware.ExecuteAsync(context,
            async ctx =>
            {
                result = await InvokeMiddlewarePipeline<TCommand, TResult>(command, (SettableCommandContext)ctx,
                    index + 1);
            });

        return result;
    }
}