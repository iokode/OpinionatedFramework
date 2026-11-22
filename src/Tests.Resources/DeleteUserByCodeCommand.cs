using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Resources.Attributes;

namespace IOKode.OpinionatedFramework.Tests.Resources;

[DeleteResource("user", "code")]
public class DeleteUserByCodeCommand : Command<string>
{
    public required int Code { get; init; }

    protected override Task<string> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        return Task.FromResult($"deleted-{Code}");
    }
}
