using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Resources.Attributes;

namespace IOKode.OpinionatedFramework.Tests.Resources;

[ActionOnResource("user", key: "code", action: "enable")]
public class ActionUserEnableByCodeAndMultipleParametersCommand : Command<string>
{
    public required string Code { get; init; }
    public required string EnableName { get; init; }
    public required bool Enable { get; init; }

    public override async Task<ValidationErrors> ValidateAsync(CancellationToken cancellationToken)
    {
        var errors = await ValidateInputsAsync(cancellationToken);
        if (Enable && string.IsNullOrWhiteSpace(EnableName))
        {
            errors.AddForObject("A name is required to enable a user.");
        }

        return errors;
    }

    protected override Task<string> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        return Task.FromResult($"enabled-by-code-{Code}-{EnableName}-{Enable}");
    }
}
