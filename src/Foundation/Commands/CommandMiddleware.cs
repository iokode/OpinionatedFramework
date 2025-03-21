using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Commands;

public delegate Task InvokeNextMiddlewareDelegate();

public abstract class CommandMiddleware
{
    public abstract Task ExecuteAsync(ICommandExecutionContext executionContext, InvokeNextMiddlewareDelegate nextAsync);
}
