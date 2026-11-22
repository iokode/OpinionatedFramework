using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Resources.Attributes;

namespace IOKode.OpinionatedFramework.Tests.Resources;

[ActionOnResource("user", "rename", "name")]
public class ActionUserRenameByNameCommand : Command<string>
{
    public required string Name { get; init; }
    public required string NewName { get; init; }

    protected override Task<string> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        return Task.FromResult($"{Name}->{NewName}");
    }
}
