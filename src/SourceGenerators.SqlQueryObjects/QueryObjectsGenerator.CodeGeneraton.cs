using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Humanizer;
using IOKode.OpinionatedFramework.Persistence.Queries;
using IOKode.OpinionatedFramework.Utilities;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace IOKode.OpinionatedFramework.SourceGenerators.SqlQueryObjects;

public partial class QueryObjectsGenerator
{
    private enum ResultSetShape
    {
        Object,
        Scalar
    }

    private class ConfigOptions
    {
        public required string RootNamespace { get; set; }
        public required string RootPath { get; set; }
    }

    private class SqlFile
    {
        public required string FilePath { get; set; }
        public required string Content { get; set; }
    }

    private class QueryObjectClass
    {
        public SqlFile SqlFile { get; }
        public ConfigOptions ConfigOptions { get; }

        public bool HasMapDirective { get; private set; }
        public bool IsInternal { get; private set; }
        public bool HasExplicitResultSets { get; private set; }

        public string[] Usings { get; private set; }
        public string[] GlobalDirectives { get; private set; }
        public ParameterType[] QueryParameters { get; private set; }
        public ParameterType[] InputParameters { get; private set; }
        public string[] Attributes { get; private set; }
        public ResultSetDefinition[] ResultSets { get; private set; }

        public string? MappedQueryResultString { get; set; }
        public bool HasMappedParameters { get; private set; }
        public bool HasMappedResult => !string.IsNullOrWhiteSpace(MappedQueryResultString);
        public bool RequiresCustomMapParameters => HasMapDirective && HasMappedParameters;
        public bool RequiresCustomMapResult => !HasExplicitResultSets && HasMapDirective && (!HasMappedParameters || HasMappedResult);

        public string Name => Path.GetFileNameWithoutExtension(SqlFile.FilePath);
        public string ClassName => Name.EndsWith("Query") ? Name : $"{Name}Query";
        public string FileName => $"{ClassName}.g.cs";

        public string Namespace => _GetNamespace();
        public string QueryContent => SqlFile.Content;
        public ResultSetDefinition SingleResultSet => ResultSets[0];
        public string CompositeQueryResultClassName => $"{ClassName}Result";

        public string QueryParametersString
        {
            get
            {
                var queryParameters = string.Join(", ", QueryParameters.Select(parameter => $"{parameter.Type} {parameter.CamelCaseName}"));
                if (!string.IsNullOrWhiteSpace(queryParameters))
                {
                    queryParameters += ", ";
                }

                queryParameters += "CancellationToken cancellationToken";
                return queryParameters;
            }
        }

        public string QueryResultString => HasExplicitResultSets
            ? CompositeQueryResultClassName
            : SingleResultSet.ResultString;

        public string InvokeResultString => RequiresCustomMapResult && !string.IsNullOrWhiteSpace(MappedQueryResultString)
            ? MappedQueryResultString!
            : QueryResultString;

        public string QueryClassAccessor => IsInternal ? "internal" : "public";
        public string QueryParametersClassName => $"{ClassName}Parameters";

        private readonly string _QueryParameterRegex = @"--\s*@parameter[ \t]([^\n]+)[ \t]+(\S+)";
        private readonly string _QueryNamespaceParameterRegex = @"--\s*@namespace[ \t]+(\S+)";
        private readonly string _QueryUsingRegex = @"--\s*@using[ \t]+([^\n]+)";
        private readonly string _QueryAttributeRegex = @"--\s*@attribute[ \t]([^\n]+)\s*";
        private readonly string _QueryInternalRegex = @"--\s*@internal";
        private readonly string _QueryMapRegex = @"--\s*@map(?:[ \t]+([^\n]*?))?(?:[ \t]*->[ \t]*([^\n]+))?\s*$";

        public QueryObjectClass(SqlFile sqlFile, ConfigOptions configOptions)
        {
            SqlFile = sqlFile;
            ConfigOptions = configOptions;

            HasMapDirective = Regex.IsMatch(sqlFile.Content, _QueryMapRegex, RegexOptions.Multiline);
            IsInternal = Regex.IsMatch(sqlFile.Content, _QueryInternalRegex);

            var queryUsingMatches = Regex.Matches(sqlFile.Content, _QueryUsingRegex);
            var queryParameterMatches = Regex.Matches(sqlFile.Content, _QueryParameterRegex);
            var queryAttributeMatches = Regex.Matches(sqlFile.Content, _QueryAttributeRegex);
            var queryMapMatches = Regex.Matches(sqlFile.Content, _QueryMapRegex, RegexOptions.Multiline);

            Usings = queryUsingMatches.Cast<Match>().Select(match => match.Groups[1].Value).ToArray();
            GlobalDirectives = SqlUtilities.GetQueryBlocks(sqlFile.Content).GlobalDirectives
                .Select(EscapeStringLiteralContent)
                .ToArray();
            QueryParameters = queryParameterMatches
                .Cast<Match>()
                .Select(match => new ParameterType
                {
                    Type = SyntaxFactory.ParseTypeName(match.Groups[1].Value).ToString(),
                    Name = SyntaxFactory.ParseName(match.Groups[2].Value).ToString(),
                })
                .ToArray();

            var mappedParametersString = queryMapMatches.Cast<Match>().FirstOrDefault()?.Groups[1].Value;
            MappedQueryResultString = queryMapMatches.Cast<Match>().FirstOrDefault()?.Groups[2].Value;
            HasMappedParameters = HasMapDirective && !string.IsNullOrWhiteSpace(mappedParametersString);
            InputParameters = HasMappedParameters
                ? ParseParameterTypes(mappedParametersString!)
                : QueryParameters;

            Attributes = queryAttributeMatches
                .Cast<Match>()
                .Select(match =>
                {
                    var code = $"{match.Groups[1].Value} class __GEN {{ }}";
                    var tree = CSharpSyntaxTree.ParseText(code);
                    var root = tree.GetRoot();
                    var attribute = root.DescendantNodes().OfType<AttributeListSyntax>().FirstOrDefault();
                    return attribute?.ToString();
                })
                .Where(attribute => !string.IsNullOrWhiteSpace(attribute))
                .Cast<string>()
                .ToArray();

            ResultSets = ParseResultSets(sqlFile.Content);
        }

        private ResultSetDefinition[] ParseResultSets(string content)
        {
            var queryBlocks = SqlUtilities.GetQueryBlocks(content);
            HasExplicitResultSets = queryBlocks.HasExplicitResultSets;
            if (!HasExplicitResultSets)
            {
                return new[] { ResultSetDefinition.ParseImplicit(queryBlocks.ResultSets[0], $"{ClassName}Result") };
            }

            return queryBlocks.ResultSets
                .Select((block, index) => ResultSetDefinition.ParseExplicit(block, index))
                .ToArray();
        }

        private string _GetNamespace()
        {
            var queryNamespaceMatches = Regex.Matches(SqlFile.Content, _QueryNamespaceParameterRegex);
            var namespaceInFile = queryNamespaceMatches.Cast<Match>().FirstOrDefault()?.Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(namespaceInFile))
            {
                return namespaceInFile!;
            }

            var rootPathName = ConfigOptions.RootPath.TrimEnd(Path.DirectorySeparatorChar);
            var fileDirectoryPath = Path.GetDirectoryName(SqlFile.FilePath) ?? string.Empty;
            int startRootPathIndex = fileDirectoryPath.IndexOf(rootPathName, StringComparison.InvariantCultureIgnoreCase);
            string relativePath = fileDirectoryPath.Substring(startRootPathIndex + rootPathName.Length);
            string relativeNamespace = relativePath.Replace(Path.DirectorySeparatorChar, '.').Trim('.');

            return string.IsNullOrWhiteSpace(relativeNamespace)
                ? ConfigOptions.RootNamespace
                : $"{ConfigOptions.RootNamespace}.{relativeNamespace}";
        }

        private static ParameterType[] ParseParameterTypes(string parametersString)
        {
            return parametersString
                .Split(',')
                .Select(parameter => parameter.Trim())
                .Where(parameter => !string.IsNullOrWhiteSpace(parameter))
                .Select(parameter =>
                {
                    var match = Regex.Match(parameter, @"^(.+)\s+(\S+)$");
                    return new ParameterType
                    {
                        Type = SyntaxFactory.ParseTypeName(match.Groups[1].Value).ToString(),
                        Name = SyntaxFactory.ParseName(match.Groups[2].Value).ToString(),
                    };
                })
                .ToArray();
        }

    }

    private class ResultSetDefinition
    {
        public required string? Name { get; init; }
        public required int Index { get; init; }
        public required string RawSql { get; init; }
        public required string[] Directives { get; init; }
        public required QueryCardinality Cardinality { get; init; }
        public required ResultSetShape Shape { get; init; }
        public required ParameterType[] ResultParameters { get; init; }
        public required ParameterType? ScalarResult { get; init; }
        public required string ObjectResultClassName { get; init; }

        public bool IsScalar => Shape == ResultSetShape.Scalar;
        public string PropertyName => Name!.Pascalize();
        public string NameLiteral => Name == null ? "null" : $"\"{Name}\"";
        public string CardinalityString => $"QueryCardinality.{Cardinality}";
        public string ShapeString => $"QueryResultShape.{Shape}";
        public string ElementType => IsScalar ? ScalarResult!.Type : ObjectResultClassName;
        public string ElementTypeForTypeOf => IsScalar ? ScalarResult!.TypeForTypeOf : ObjectResultClassName;
        public string ScalarColumnName => ScalarResult?.Name ?? string.Empty;
        public string ResultString => Cardinality switch
        {
            QueryCardinality.One => ElementType,
            QueryCardinality.ZeroOrOne => $"{ElementType}?",
            _ => $"IReadOnlyCollection<{ElementType}>"
        };

        public static ResultSetDefinition ParseImplicit(SqlQueryResultSetBlock block, string objectResultClassName)
        {
            return Parse(block, null, 0, objectResultClassName);
        }

        public static ResultSetDefinition ParseExplicit(SqlQueryResultSetBlock block, int index)
        {
            var resultSetMatch = Regex.Match(block.RawSql, @"--\s*@result_set[ \t]+(\S+)");
            if (!resultSetMatch.Success)
            {
                throw new QueryDefinitionException("Explicit result sets must declare a name.");
            }

            var resultSetName = resultSetMatch.Groups[1].Value;
            var objectResultClassName = $"{resultSetName.Singularize().Pascalize()}Result";
            return Parse(block, resultSetName, index, objectResultClassName);
        }

        private static ResultSetDefinition Parse(SqlQueryResultSetBlock block, string? name, int index, string objectResultClassName)
        {
            var resultMatches = Regex.Matches(block.RawSql, @"--\s*@result[ \t]([^\n]+)[ \t]+(\S+)");
            var scalarResultMatches = Regex.Matches(block.RawSql, @"--\s*@scalar_result[ \t]([^\n]+)[ \t]+(\S+)");

            if (resultMatches.Count > 0 && scalarResultMatches.Count > 0)
            {
                throw new QueryDefinitionException("A result set cannot declare both @result and @scalar_result.");
            }

            if (scalarResultMatches.Count > 1)
            {
                throw new QueryDefinitionException("A result set cannot declare more than one @scalar_result.");
            }

            if (resultMatches.Count == 0 && scalarResultMatches.Count == 0)
            {
                throw new QueryDefinitionException("A result set must declare @result or @scalar_result.");
            }

            var cardinality = ParseCardinality(block);
            var resultParameters = resultMatches
                .Cast<Match>()
                .Select(match => new ParameterType
                {
                    Type = SyntaxFactory.ParseTypeName(match.Groups[1].Value).ToString(),
                    Name = SyntaxFactory.ParseName(match.Groups[2].Value).ToString(),
                })
                .ToArray();
            var scalarResult = scalarResultMatches
                .Cast<Match>()
                .Select(match => new ParameterType
                {
                    Type = SyntaxFactory.ParseTypeName(match.Groups[1].Value).ToString(),
                    Name = SyntaxFactory.ParseName(match.Groups[2].Value).ToString(),
                })
                .FirstOrDefault();

            return new ResultSetDefinition
            {
                Name = name,
                Index = index,
                RawSql = block.RawSql,
                Directives = block.Directives
                    .Select(EscapeStringLiteralContent)
                    .ToArray(),
                Cardinality = cardinality,
                Shape = scalarResult != null ? ResultSetShape.Scalar : ResultSetShape.Object,
                ResultParameters = resultParameters,
                ScalarResult = scalarResult,
                ObjectResultClassName = objectResultClassName,
            };
        }

        private static QueryCardinality ParseCardinality(SqlQueryResultSetBlock block)
        {
            return SqlUtilities.GetCardinality(block.Directives);
        }
    }

    private class ParameterType
    {
        public required string Name { get; set; }
        public required string Type { get; set; }
        public string PascalCaseName => Name.Pascalize();
        public string CamelCaseName => Name.Camelize();
        public string RequiredKeyword => Type.EndsWith("?") ? string.Empty : "required ";
        public string TypeForTypeOf => Type.EndsWith("?") ? Type[..^1] : Type;
    }

    private static string EscapeStringLiteralContent(string value)
    {
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    private const string _QueryObjectClassTemplate =
        """"
        // This file was auto-generated by a source generator

        #nullable enable
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Threading;
        using System.Threading.Tasks;
        using IOKode.OpinionatedFramework;
        using IOKode.OpinionatedFramework.Persistence.Queries;
        {{~ for using in Usings ~}}
        using {{ using }};
        {{~ end ~}}

        namespace {{ Namespace }};

        {{~ for attribute in Attributes ~}}
        {{ attribute }}
        {{~ end ~}}
        {{ QueryClassAccessor }} partial class {{ ClassName }} : IQuery<{{ InvokeResultString }}>
        {
            public const string Query =
                """
                {{ QueryContent }}
                """;

            private static readonly IReadOnlyList<string> directives =
            [
                {{~ for directive in GlobalDirectives ~}}
                "{{ directive }}",
                {{~ end ~}}
            ];
            private static readonly IReadOnlyList<QueryResultSet> resultSets =
            [
                {{~ for result_set in ResultSets ~}}
                new QueryResultSet
                {
                    Name = {{ result_set.NameLiteral }},
                    RawSql =
                        """
                        {{ result_set.RawSql }}
                        """,
                    Directives =
                    [
                        {{~ for directive in result_set.Directives ~}}
                        "{{ directive }}",
                        {{~ end ~}}
                    ],
                    Cardinality = {{ result_set.CardinalityString }},
                    Shape = {{ result_set.ShapeString }},
                    ResultType = typeof({{ result_set.ElementTypeForTypeOf }}),
                    ScalarColumnName = {{ if result_set.IsScalar }}"{{ result_set.ScalarColumnName }}"{{ else }}null{{ end }}
                },
                {{~ end ~}}
            ];

            public string RawSql => Query;

            public IReadOnlyList<string> Directives => directives;

            public IReadOnlyList<QueryResultSet> ResultSets => resultSets;

            {{~ for parameter in InputParameters ~}}
            public {{ parameter.RequiredKeyword }}{{ parameter.Type }} {{ parameter.PascalCaseName }} { get; init; }
            {{~ end ~}}

            {{~ if RequiresCustomMapParameters ~}}
            private partial {{ QueryParametersClassName }} MapParameters();
            {{~ else ~}}
            private {{ QueryParametersClassName }} MapParameters()
            {
                return new {{ QueryParametersClassName }}
                {
                    {{~ for parameter in QueryParameters ~}}
                    {{ parameter.PascalCaseName }} = this.{{ parameter.PascalCaseName }},
                    {{~ end ~}}
                };
            }
            {{~ end ~}}

            {{~ if HasExplicitResultSets ~}}
            private {{ InvokeResultString }} MapResult(IReadOnlyList<object> rawResultSets)
            {
                return new {{ InvokeResultString }}
                {
                    {{~ for result_set in ResultSets ~}}
                    {{ result_set.PropertyName }} = Map{{ result_set.Cardinality }}(GetResultSet<{{ result_set.ElementType }}>(rawResultSets, {{ result_set.Index }})),
                    {{~ end ~}}
                };
            }
            {{~ else if RequiresCustomMapResult ~}}
            private partial {{ InvokeResultString }} MapResult(IReadOnlyCollection<{{ SingleResultSet.ElementType }}> rawResults);
            {{~ else ~}}
            private {{ InvokeResultString }} MapResult(IReadOnlyCollection<{{ SingleResultSet.ElementType }}> rawResults)
            {
                return Map{{ SingleResultSet.Cardinality }}(rawResults);
            }
            {{~ end ~}}

            private static IReadOnlyCollection<T> GetResultSet<T>(IReadOnlyList<object> rawResultSets, int index)
            {
                return (IReadOnlyCollection<T>)rawResultSets[index];
            }

            private static IReadOnlyCollection<T> MapZeroOrMore<T>(IReadOnlyCollection<T> rawResults)
            {
                return rawResults;
            }

            private static T MapOne<T>(IReadOnlyCollection<T> rawResults)
            {
                return rawResults.First();
            }

            private static T? MapZeroOrOne<T>(IReadOnlyCollection<T> rawResults)
            {
                return rawResults.FirstOrDefault();
            }

        }

        {{~ if HasExplicitResultSets ~}}
        {{ QueryClassAccessor }} partial record {{ CompositeQueryResultClassName }}
        {
            {{~ for result_set in ResultSets ~}}
            public required {{ result_set.ResultString }} {{ result_set.PropertyName }} { get; init; }
            {{~ end ~}}
        }
        {{~ end ~}}

        {{~ for result_set in ResultSets ~}}
        {{~ if !result_set.IsScalar ~}}
        {{ QueryClassAccessor }} partial record {{ result_set.ObjectResultClassName }}
        {
            {{~ for parameter in result_set.ResultParameters ~}}
            public required {{ parameter.Type }} {{ parameter.PascalCaseName }} { get; init; }
            {{~ end ~}}
        }
        {{~ end ~}}
        {{~ end ~}}
        
        {{ QueryClassAccessor }} partial record {{ QueryParametersClassName }}
        {
            {{~ for parameter in QueryParameters ~}}
            public {{ parameter.RequiredKeyword }}{{ parameter.Type }} {{ parameter.PascalCaseName }} { get; init; }
            {{~ end ~}}
        }
        """";
}
