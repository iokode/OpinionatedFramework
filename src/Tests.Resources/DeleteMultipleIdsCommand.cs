using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Resources.Attributes;

namespace IOKode.OpinionatedFramework.Tests.Resources;

[DeleteResource("user")]
public class DeleteMultipleIdsCommand : Command<string>
{
    public required int[] Ids { get; init; }

    protected override Task<string> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        var stringIds = string.Join(",", Ids);
        return Task.FromResult($"deleted-{stringIds}");
    }
}
