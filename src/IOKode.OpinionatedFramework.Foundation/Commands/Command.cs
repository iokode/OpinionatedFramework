using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Commands;

public abstract class Command
{
    protected abstract Task ExecuteAsync(CancellationToken cancellationToken);
}

public abstract class Command<TResult>
{
    protected abstract Task<TResult> ExecuteAsync(CancellationToken cancellationToken);
}