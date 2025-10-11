using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Resources.Attributes;

namespace IOKode.OpinionatedFramework.Tests.Resources;

[UpdateResource("user")]
public class UpdateUserCommand(int id) : Command<string>
{
    protected override Task<string> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        return Task.FromResult($"updated-{id}");
    }
}
