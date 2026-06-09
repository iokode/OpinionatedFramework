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
    private static readonly Regex directivePrefixRegex = new(@"--[ \t]*@", RegexOptions.Compiled);
    
    private static readonly IReadOnlyDictionary<string, QueryCardinality> cardinalityDirectives =
        new Dictionary<string, QueryCardinality>(StringComparer.OrdinalIgnoreCase)
        {
            ["zero_or_more"] = QueryCardinality.ZeroOrMore,
            ["one"] = QueryCardinality.One,
            ["zero_or_one"] = QueryCardinality.ZeroOrOne,
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
    /// Splits raw SQL text into global directives and result set blocks.
    /// </summary>
    /// <remarks>
    /// When explicit result sets are declared, global directives are the directives that appear before the first
    /// <c>@result_set</c> directive. Result set directives are the directives that appear within each result set block.
    /// When no explicit result set is declared, all directives are global and also belong to the implicit result set.
    /// </remarks>
    /// <param name="rawSql">The raw SQL text.</param>
    /// <returns>The global SQL block and result set SQL blocks.</returns>
    public static SqlQueryBlocks GetQueryBlocks(string rawSql)
    {
        var lines = rawSql.Replace("\r\n", "\n").Split('\n');
        var resultSetLineIndexes = lines
            .Select((line, index) => new { line, index })
            .Where(item => IsResultSetDirectiveLine(item.line))
            .Select(item => item.index)
            .ToArray();

        if (resultSetLineIndexes.Length == 0)
        {
            return new SqlQueryBlocks
            {
                GlobalRawSql = rawSql,
                GlobalDirectives = GetDirectives(rawSql),
                HasExplicitResultSets = false,
                ResultSets = new[]
                {
                    new SqlQueryResultSetBlock
                    {
                        RawSql = rawSql,
                        Directives = GetDirectives(rawSql)
                    }
                }
            };
        }

        var globalRawSql = string.Join("\n", lines.Take(resultSetLineIndexes[0]));
        var resultSets = resultSetLineIndexes
            .Select((lineIndex, index) =>
            {
                var nextLineIndex = index + 1 < resultSetLineIndexes.Length ? resultSetLineIndexes[index + 1] : lines.Length;
                var block = string.Join("\n", lines.Skip(lineIndex).Take(nextLineIndex - lineIndex));
                return new SqlQueryResultSetBlock
                {
                    RawSql = block,
                    Directives = GetDirectives(block)
                };
            })
            .ToArray();

        return new SqlQueryBlocks
        {
            GlobalRawSql = globalRawSql,
            GlobalDirectives = GetDirectives(globalRawSql),
            HasExplicitResultSets = true,
            ResultSets = resultSets
        };
    }

    /// <summary>
    /// Gets the query cardinality represented by the provided directives.
    /// </summary>
    /// <param name="directives">The directives associated with a query.</param>
    /// <returns>The cardinality represented by the directives, or <see cref="QueryCardinality.ZeroOrMore"/> when no cardinality directive is present.</returns>
    /// <exception cref="QueryDefinitionException">Thrown when the provided directives contain incompatible cardinalities.</exception>
    public static QueryCardinality GetCardinality(IReadOnlyList<string> directives)
    {
        var cardinalities = directives
            .Select(GetCardinalityDirectiveValue)
            .Where(cardinalityValue => cardinalityValue != null && cardinalityDirectives.ContainsKey(cardinalityValue))
            .Select(cardinalityValue => cardinalityDirectives[cardinalityValue!])
            .Distinct()
            .ToArray();

        if (cardinalities.Length > 1)
        {
            throw new QueryDefinitionException($"Incompatible query cardinality directives: {string.Join(", ", cardinalities)}.");
        }

        return cardinalities.Length == 0 ? QueryCardinality.ZeroOrMore : cardinalities[0];
    }

    private static string? GetCardinalityDirectiveValue(string directive)
    {
        var directiveParts = directive.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (directiveParts.Length != 2 || !directiveParts[0].Equals("cardinality", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return directiveParts[1];
    }

    private static bool IsResultSetDirectiveLine(string line)
    {
        var trimmedLine = line.Trim();
        var match = directivePrefixRegex.Match(trimmedLine);
        if (!match.Success)
        {
            return false;
        }

        var directive = trimmedLine[match.Length..].Trim();
        var directiveName = directive.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return directiveName == "result_set";
    }
}
