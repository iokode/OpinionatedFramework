using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Commands;

public abstract class Command
{
    protected virtual Task PrepareAsync(CommandContext context)
    {
        return Task.CompletedTask;
    }

    protected abstract Task ExecuteAsync(CommandContext context);
}

public abstract class Command<TResult>
{
    protected virtual Task PrepareAsync(CommandContext context)
    {
        return Task.CompletedTask;
    }
    
    protected abstract Task<TResult> ExecuteAsync(CommandContext context);
}