using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Resources.Attributes;

namespace IOKode.OpinionatedFramework.Tests.Resources;

[ReplaceResource("user", "by code")]
public class ReplaceUserByCodeCommand(int id) : Command<string>
{
    protected override Task<string> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        return Task.FromResult($"replaced-{id}");
    }
}