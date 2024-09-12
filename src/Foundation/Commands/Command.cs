using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Commands;

public abstract class Command
{
    protected virtual Task PrepareAsync(ICommandContext context)
    {
        return Task.CompletedTask;
    }

    protected abstract Task ExecuteAsync(ICommandContext context);
}

public abstract class Command<TResult>
{
    protected virtual Task PrepareAsync(ICommandContext context)
    {
        return Task.CompletedTask;
    }
    
    protected abstract Task<TResult> ExecuteAsync(ICommandContext context);
}