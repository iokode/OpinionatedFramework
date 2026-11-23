using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Resources.Attributes;

namespace IOKode.OpinionatedFramework.Tests.Resources;

[CreateResource("user")]
public class CreateUserCommand : Command<CreateUserOutput>
{
    public required string Name { get; init; }
    public required IReadOnlyList<CreateUserAddressInput> Addresses { get; init; }
    public required IReadOnlyDictionary<string, CreateUserPhoneInput> Phones { get; init; }

    public override async Task ValidateAsync(ValidationErrors errors, CancellationToken cancellationToken)
    {
        await ValidateNestedInputsAsync(errors, cancellationToken);
        if (string.IsNullOrWhiteSpace(Name))
        {
            errors.Add(nameof(Name), "A name is required.");
        }
    }

    protected override Task<CreateUserOutput> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        return Task.FromResult(new CreateUserOutput
        {
            Id = Name
        });
    }
}

public class CreateUserAddressInput : Input
{
    public required string City { get; init; }

    public override async Task ValidateAsync(ValidationErrors errors, CancellationToken cancellationToken)
    {
        await ValidateNestedInputsAsync(errors, cancellationToken);
        if (string.IsNullOrWhiteSpace(City))
        {
            errors.Add(nameof(City), "A city is required.");
        }
    }
}

public class CreateUserPhoneInput : Input
{
    public required string Number { get; init; }

    public override async Task ValidateAsync(ValidationErrors errors, CancellationToken cancellationToken)
    {
        await ValidateNestedInputsAsync(errors, cancellationToken);
        if (string.IsNullOrWhiteSpace(Number))
        {
            errors.Add(nameof(Number), "A phone number is required.");
        }
    }
}

public record CreateUserOutput
{
    public required string Id { get; init; }
}
