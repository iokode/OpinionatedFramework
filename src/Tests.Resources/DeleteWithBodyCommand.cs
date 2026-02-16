using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Resources.Attributes;

namespace IOKode.OpinionatedFramework.Tests.Resources;

[DeleteResource("user")]
public class DeleteWithBodyCommand(int[] ids) : Command<string>
{
    protected override Task<string> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        var stringIds = string.Join(",", ids);
        return Task.FromResult($"deleted-{stringIds}");
    }
}