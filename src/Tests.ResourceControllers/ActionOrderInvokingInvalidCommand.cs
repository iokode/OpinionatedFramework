using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Commands.Extensions;
using IOKode.OpinionatedFramework.Resources.Attributes;

namespace IOKode.OpinionatedFramework.Tests.ResourceControllers;

/// <summary>
/// A valid command that, while running, invokes another command with data that is not valid: that is not a problem of
/// the data of the request.
/// </summary>
[ActionOnResource("order", "invoke invalid")]
public class ActionOrderInvokingInvalidCommand : Command
{
    protected override async Task ExecuteAsync(ICommandExecutionContext executionContext)
    {
        await new InnerCommand { Name = "" }.InvokeAsync(executionContext.CancellationToken);
    }

    private class InnerCommand : Command
    {
        public required string Name { get; init; }

        public override Task<ValidationErrors> ValidateAsync(CancellationToken cancellationToken)
        {
            var errors = new ValidationErrors();
            errors.Add(nameof(Name), "A name is required.");
            return Task.FromResult(errors);
        }

        protected override Task ExecuteAsync(ICommandExecutionContext executionContext)
        {
            return Task.CompletedTask;
        }
    }
}
