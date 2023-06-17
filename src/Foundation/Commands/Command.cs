using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Commands;

public abstract class Command
{
    protected abstract Task ExecuteAsync(CommandContext context);
}

public abstract class Command<TResult>
{
    protected abstract Task<TResult> ExecuteAsync(CommandContext context);
}