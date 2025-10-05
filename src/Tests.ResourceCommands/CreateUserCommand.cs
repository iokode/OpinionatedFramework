using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Resources.Attributes;

namespace IOKode.OpinionatedFramework.Tests.ResourceCommands;

[Create("user")]
public class CreateUserCommand(CreateUserInput input) : Command<CreateUserOutput>
{
    protected override Task<CreateUserOutput> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        return Task.FromResult(new CreateUserOutput
        {
            Id = input.Name
        });
    }
}

public class CreateUserInput
{
    public required string Name { get; set; }
}

public record CreateUserOutput
{
    public required string Id { get; init; }
}