using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Resources.Attributes;

namespace IOKode.OpinionatedFramework.Tests.Resources;

[RetrieveResource("user", "code")]
public class RetrieveUserByCodeCommand : Command<string>
{
    public required int Code { get; init; }

    public override async Task<ValidationErrors> ValidateAsync(CancellationToken cancellationToken)
    {
        var errors = await ValidateInputsAsync(cancellationToken);
        if (Code < 1)
        {
            errors.Add(nameof(Code), "The code must be positive.");
        }

        return errors;
    }

    protected override Task<string> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        return Task.FromResult(Code.ToString());
    }
}
