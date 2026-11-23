using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Resources.Attributes;

namespace IOKode.OpinionatedFramework.Tests.Resources;

[ListResources("user/by key", "/key1,Key2")]
public class ListUsersSubResourceByKeysAndQueryParametersCommand : Command<string[]>
{
    public required string Key1 { get; init; }
    public required int Key2 { get; init; }
    public int? Param1 { get; init; }
    public bool? Param2 { get; init; }

    public override async Task ValidateAsync(ValidationErrors errors, CancellationToken cancellationToken)
    {
        await ValidateNestedInputsAsync(errors, cancellationToken);
        if (Param1 > 100)
        {
            errors.Add(nameof(Param1), "The first parameter cannot be greater than 100.");
        }
    }

    protected override Task<string[]> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        List<string> results = [Key1, Key2.ToString()];

        if (Param1.HasValue)
        {
            results.Add(Param1.Value.ToString());
        }

        if (Param2.HasValue)
        {
            results.Add(Param2.Value.ToString());
        }

        return Task.FromResult(results.ToArray());
    }
}
