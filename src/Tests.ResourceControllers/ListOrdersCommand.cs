using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Resources.Attributes;

namespace IOKode.OpinionatedFramework.Tests.ResourceControllers;

[ListResources("order")]
public class ListOrdersCommand : Command<string[]>
{
    protected override Task<string[]> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        return Task.FromResult(new[] { "order" });
    }
}

/// <summary>
/// The same operation on the same resource as <see cref="ListOrdersCommand"/>, generated in its own controller
/// (<c>PublicOrderResourceController</c>) because it belongs to another group.
/// </summary>
[ListResources("order", group: "Public")]
public class ListPublicOrdersCommand : Command<string[]>
{
    protected override Task<string[]> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        return Task.FromResult(new[] { "public-order" });
    }
}
