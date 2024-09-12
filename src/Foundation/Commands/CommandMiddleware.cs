using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Commands;

public delegate Task InvokeNextMiddlewareDelegate(ICommandContext context);

public abstract class CommandMiddleware
{
    public abstract Task ExecuteAsync(ICommandContext context, InvokeNextMiddlewareDelegate nextAsync);
}
