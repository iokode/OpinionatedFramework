using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Foundation.Commands;

public abstract class Command
{
    public abstract Task ExecuteAsync();
}

public abstract class Command<TResult>
{
    public abstract Task<TResult> ExecuteAsync();
}