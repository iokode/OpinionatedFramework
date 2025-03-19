using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Commands;

public abstract class Command
{
    protected virtual Task PrepareAsync(ICommandExecutionContext executionContext)
    {
        return Task.CompletedTask;
    }

    protected abstract Task ExecuteAsync(ICommandExecutionContext executionContext);
}

public abstract class Command<TResult>
{
    protected virtual Task PrepareAsync(ICommandExecutionContext executionContext)
    {
        return Task.CompletedTask;
    }
    
    protected abstract Task<TResult> ExecuteAsync(ICommandExecutionContext executionContext);
}