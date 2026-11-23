using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Resources.Attributes;

namespace IOKode.OpinionatedFramework.Tests.Resources;

[RetrieveResource("user", "code")]
public class RetrieveUserByCodeCommand : Command<string>
{
    public required int Code { get; init; }

    public override async Task ValidateAsync(ValidationErrors errors, CancellationToken cancellationToken)
    {
        await ValidateNestedInputsAsync(errors, cancellationToken);
        if (Code < 1)
        {
            errors.Add(nameof(Code), "The code must be positive.");
        }
    }

    protected override Task<string> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        return Task.FromResult(Code.ToString());
    }
}
