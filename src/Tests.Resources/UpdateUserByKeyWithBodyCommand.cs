using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Resources.Attributes;

namespace IOKode.OpinionatedFramework.Tests.Resources;

[UpdateResource("User", "code")]
public class UpdateUserByKeyWithBodyCommand(int code, UpdateUserByKeyWithBodyInput input) : Command<string>
{
    protected override Task<string> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        return Task.FromResult($"updated-{code}-{input.Updated}");
    }
}

public record UpdateUserByKeyWithBodyInput(bool Updated);