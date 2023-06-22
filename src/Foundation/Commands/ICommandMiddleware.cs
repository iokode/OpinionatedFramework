using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Commands;

public delegate Task InvokeNextMiddlewareDelegate(CommandContext context);

public interface ICommandMiddleware
{
    public Task ExecuteAsync(CommandContext context, InvokeNextMiddlewareDelegate nextAsync);
}
