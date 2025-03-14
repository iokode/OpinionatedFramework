using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Facades;
using Command = IOKode.OpinionatedFramework.Commands.Command;

namespace IOKode.OpinionatedFramework.ContractImplementations.CommandExecutor;

public class CommandExecutor : ICommandExecutor
{
    private readonly CommandMiddleware[] middlewares;
    private readonly Dictionary<string, object?> sharedData;

    public CommandExecutor(CommandMiddleware[] middlewares,
        IEnumerable<KeyValuePair<string, object?>>? sharedData = null)
    {
        this.middlewares = middlewares;
        this.sharedData = sharedData?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ??
                          new Dictionary<string, object?>();
    }

    public async Task InvokeAsync<TCommand>(TCommand command, CancellationToken cancellationToken)
        where TCommand : Command
    {
        Log.Trace("Invoking pipeline for command '{command}'...", command.GetType());

        Container.Advanced.CreateScope();
        var context = SettableCommandContext.Create(command.GetType(), this.sharedData, cancellationToken);
        await InvokeMiddlewarePipelineAsync(command, context, 0);
        Container.Advanced.DisposeScope();

        Log.Trace("Command '{command}' invoked.", command.GetType());
    }

    public async Task<TResult> InvokeAsync<TCommand, TResult>(TCommand command,
        CancellationToken cancellationToken)
        where TCommand : Command<TResult>
    {
        Log.Info("Invoking pipeline for command '{command}'...", command.GetType());

        Container.Advanced.CreateScope();
        var context = SettableCommandContext.Create(command.GetType(), sharedData, cancellationToken);
        var result = await InvokeMiddlewarePipelineAsync<TCommand, TResult>(command, context, 0);
        Container.Advanced.DisposeScope();

        Log.Trace("Command '{command}' invoked.", command.GetType());
        return result;
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

    /// <summary>
    /// Prepare and execute command that DO NOT return any result.
    /// </summary>
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
        // No need to set the shared data again because it uses the same reference.
    }

    /// <summary>
    /// Prepare and execute command that DO return a result.
    /// </summary>
    private async Task<TResult> PrepareAndExecuteAsync<TCommand, TResult>(TCommand command,
        SettableCommandContext context)
    {
        var prepare = GetPrepareMethod<TCommand>();
        var execute = GetExecuteMethod<TCommand>();
        var parameters = new object?[] { context };

        TResult result;
        try
        {
            await (Task)prepare.Invoke(command, parameters)!;
            result = await (Task<TResult>)execute.Invoke(command, parameters)!;
        }
        catch (TargetInvocationException ex)
        {
            throw ex.InnerException!;
        }

        context.SetAsExecuted();
        context.SetResult(result);
        // No need to set the shared data again because it uses the same reference.

        return result;
    }

    private async Task InvokeMiddlewarePipelineAsync<TCommand>(TCommand command, SettableCommandContext context, int index)
    {
        if (index >= middlewares.Length)
        {
            await PrepareAndExecuteAsync(command, context);
            return;
        }

        var middleware = middlewares[index];
        await middleware.ExecuteAsync(context,
            () => InvokeMiddlewarePipelineAsync(command, context, index + 1));
    }

    private async Task<TResult> InvokeMiddlewarePipelineAsync<TCommand, TResult>(TCommand command,
        SettableCommandContext context, int index)
        where TCommand : Command<TResult>
    {
        TResult result = default!;
        if (index >= middlewares.Length)
        {
            return await PrepareAndExecuteAsync<TCommand, TResult>(command, context);
        }

        var middleware = middlewares[index];
        await middleware.ExecuteAsync(context,
            async () =>
            {
                result = await InvokeMiddlewarePipelineAsync<TCommand, TResult>(command, context,
                    index + 1);
            });

        return result;
    }
}