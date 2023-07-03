using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Commands;

public delegate Task InvokeNextMiddlewareDelegate(CommandContext context);

public abstract class CommandMiddleware
{
    public abstract Task ExecuteAsync(CommandContext context, InvokeNextMiddlewareDelegate nextAsync);
}
