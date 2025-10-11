using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Resources.Attributes;

namespace IOKode.OpinionatedFramework.Tests.Resources;

[ListResources("user")]
public class ListUsersCommand : Command<string[]>
{
    protected override Task<string[]> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        return Task.FromResult(new[] {"user1", "user2", "user3"});
    }
}
