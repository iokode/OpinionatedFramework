using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Commands;

public static class CommandExtensions
{
    public static async Task InvokeAsync(this Command command, CancellationToken cancellationToken = default)
    {
        await Facades.Command.InvokeAsync(command, cancellationToken);
    }

    public static async Task<T> InvokeAsync<T>(this Command<T> command, CancellationToken cancellationToken = default)
    {
        return await Facades.Command.InvokeAsync<Command<T>, T>(command, cancellationToken);
    }
}