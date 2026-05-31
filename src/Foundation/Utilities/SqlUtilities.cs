using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using IOKode.OpinionatedFramework.Persistence.Queries;

namespace IOKode.OpinionatedFramework.Utilities;

/// <summary>
/// Provides utility methods for working with SQL text.
/// </summary>
public static partial class SqlUtilities
{
    private static readonly IReadOnlyDictionary<string, QueryCardinality> cardinalityDirectives =
        new Dictionary<string, QueryCardinality>(StringComparer.OrdinalIgnoreCase)
        {
            ["single"] = QueryCardinality.One,
            ["single_or_default"] = QueryCardinality.ZeroOrOne,
        };

    /// <summary>
    /// Extracts SQL directives from raw SQL text.
    /// </summary>
    /// <param name="rawSql">The raw SQL text.</param>
    /// <returns>The directives found in the SQL text, without the SQL comment and directive marker.</returns>
    public static IReadOnlyList<string> GetDirectives(string rawSql)
    {
        var directives = new List<string>();
        var lines = rawSql.Split('\n');
        var directivePrefixRegex = GetDirectivePrefixRegex();

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            var match = directivePrefixRegex.Match(trimmedLine);
            if (!match.Success)
            {
                continue;
            }

            var directive = trimmedLine[match.Length..].Trim();
            directives.Add(directive);
        }

        return directives;
    }

    /// <summary>
    /// Gets the query cardinality represented by the provided directives.
    /// </summary>
    /// <param name="directives">The directives associated with a query.</param>
    /// <returns>The cardinality represented by the directives, or <see cref="QueryCardinality.ZeroOrMore"/> when no cardinality directive is present.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the provided directives contain incompatible cardinalities.</exception>
    public static QueryCardinality GetCardinality(IReadOnlyList<string> directives)
    {
        var cardinalities = directives
            .Select(GetDirectiveName)
            .Where(directiveName => directiveName != null && cardinalityDirectives.ContainsKey(directiveName))
            .Select(directiveName => cardinalityDirectives[directiveName!])
            .Distinct()
            .ToArray();

        if (cardinalities.Length > 1)
        {
            throw new InvalidOperationException($"Incompatible query cardinality directives: {string.Join(", ", cardinalities)}.");
        }

        return cardinalities.FirstOrDefault(QueryCardinality.ZeroOrMore);
    }

    private static string? GetDirectiveName(string directive)
    {
        return directive.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
    }

    [GeneratedRegex(@"--[ \t]*@")]
    private static partial Regex GetDirectivePrefixRegex();
}
