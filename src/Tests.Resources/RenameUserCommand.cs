using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Resources.Attributes;

namespace IOKode.OpinionatedFramework.Tests.Resources;

[ActionOnResource( "user", "rename", "by name")]
public class RenameUserCommand(string name, string newName) : Command<string>
{
    protected override Task<string> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        return Task.FromResult(newName);
    }
}