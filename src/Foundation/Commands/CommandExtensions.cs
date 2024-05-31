using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Commands.Extensions;

public static class CommandExtensions
{
    public static async Task InvokeAsync(this Command command, CancellationToken cancellationToken = default)
    {
        var executor = Locator.Resolve<ICommandExecutor>();
        await executor.InvokeAsync(command, cancellationToken);
    }

    public static async Task<T> InvokeAsync<T>(this Command<T> command, CancellationToken cancellationToken = default)
    {
        var executor = Locator.Resolve<ICommandExecutor>();
        return await executor.InvokeAsync<Command<T>, T>(command, cancellationToken);
    }
}