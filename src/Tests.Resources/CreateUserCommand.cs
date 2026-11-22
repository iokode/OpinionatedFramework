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
    public IReadOnlyList<CreateUserAddressInput> Addresses { get; init; } = [];
    public IReadOnlyDictionary<string, CreateUserPhoneInput> Phones { get; init; } = new Dictionary<string, CreateUserPhoneInput>();

    public override async Task<ValidationErrors> ValidateAsync(CancellationToken cancellationToken)
    {
        var errors = await ValidateInputsAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(Name))
        {
            errors.Add(nameof(Name), "A name is required.");
        }

        return errors;
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

    public override async Task<ValidationErrors> ValidateAsync(CancellationToken cancellationToken)
    {
        var errors = await ValidateInputsAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(City))
        {
            errors.Add(nameof(City), "A city is required.");
        }

        return errors;
    }
}

public class CreateUserPhoneInput : Input
{
    public required string Number { get; init; }

    public override async Task<ValidationErrors> ValidateAsync(CancellationToken cancellationToken)
    {
        var errors = await ValidateInputsAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(Number))
        {
            errors.Add(nameof(Number), "A phone number is required.");
        }

        return errors;
    }
}

public record CreateUserOutput
{
    public required string Id { get; init; }
}
