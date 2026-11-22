using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Resources.Attributes;

namespace IOKode.OpinionatedFramework.Tests.Resources;

[UpdateResource("User", "code")]
public class UpdateUserByCodeCommand : Command<string>
{
    public required int Code { get; init; }
    public Updatable<bool> Updated { get; init; }
    public Updatable<string?> Nickname { get; init; }

    protected override Task<string> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        var updated = Updated.ShouldUpdate ? Updated.Value.ToString() : "unchanged";
        var nickname = Nickname.ShouldUpdate ? Nickname.Value ?? "null" : "unchanged";
        return Task.FromResult($"updated-{Code}-{updated}-{nickname}");
    }
}
