using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Resources.Attributes;

namespace IOKode.OpinionatedFramework.Tests.ResourceControllers;

/// <summary>
/// A command that reads from the query string required values of a reference type and of a value type; a complex type
/// with a value of a non-nullable reference type, which MVC would validate as required if it validated the query
/// string; a collection and a dictionary; and a value with an initial value.
/// </summary>
[ListResources("search result")]
public class ListSearchResultsCommand : Command<string>
{
    public required string Text { get; init; }
    public required int Page { get; init; }
    public SearchOptions? Options { get; init; }
    public string[]? Tags { get; init; }
    public Dictionary<string, int>? Weights { get; init; }
    public int Limit { get; init; } = 10;

    public override async Task ValidateAsync(ValidationErrors errors, CancellationToken cancellationToken)
    {
        await ValidateNestedInputsAsync(errors, cancellationToken);
        if (string.IsNullOrWhiteSpace(Text))
        {
            errors.Add(nameof(Text), "A text to search is required.");
        }
    }

    protected override Task<string> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        var tags = string.Join(",", Tags ?? []);
        var weights = string.Join(",", (Weights ?? []).OrderBy(weight => weight.Key).Select(weight => $"{weight.Key}:{weight.Value}"));
        return Task.FromResult($"{Text}-{Page}-{Options?.Language ?? "null"}-{Options?.Page}-[{tags}]-[{weights}]-{Limit}");
    }
}

public class SearchOptions
{
    public string Language { get; set; } = null!;
    public int Page { get; set; }
    public string[]? Labels { get; set; }
}
