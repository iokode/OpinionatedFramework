using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Humanizer;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace IOKode.OpinionatedFramework.SourceGenerators.SqlQueryObjects;

public partial class QueryObjectsGenerator
{
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
        
        public bool IsSingleResult { get; private set; }
        public bool IsSingleOrDefaultResult { get; private set; }
        public bool HasMapDirective { get; private set; }
        public bool IsInternal { get; private set; }
        public bool HasCount { get; private set; }

        public string[] Usings { get; private set; }
        public ParameterType[] QueryParameters { get; private set; }
        public ParameterType[] InputParameters { get; private set; }
        public ParameterType[] ResultParameters { get; private set;}
        public string[] Attributes { get; private set; }
        public string CountName { get; private set; } = "count";
        public string? QueryResultName { get; private set; }
        public string PascalCountName => CountName.Pascalize();

        public string? MappedQueryResultString { get; set; }
        public bool HasMappedParameters { get; private set; }
        public bool HasMappedResult => !string.IsNullOrWhiteSpace(MappedQueryResultString);
        public bool RequiresCustomMapParameters => HasMapDirective && HasMappedParameters;
        public bool RequiresCustomMapResult => HasMapDirective && (!HasMappedParameters || HasMappedResult);

        public string Name => Path.GetFileNameWithoutExtension(SqlFile.FilePath);
        public string ClassName => Name.EndsWith("Query") ? Name : $"{Name}Query";
        public string FileName => $"{ClassName}.g.cs";

        public string Namespace => _GetNamespace();
        public string QueryContent => SqlFile.Content;
        
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

        public string QueryResultString => IsSingleResult
            ? QueryResultClassName
            : IsSingleOrDefaultResult
                ? $"{QueryResultClassName}?"
                : HasCount
                    ? $"QueryResultsWithCount<{QueryResultClassName}>"
                    : $"IReadOnlyCollection<{QueryResultClassName}>";
        
        public string InvokeResultString => HasMapDirective && !string.IsNullOrWhiteSpace(MappedQueryResultString)
            ? MappedQueryResultString!
            : QueryResultString;

        public string QueryRowResultString => HasCount ? $"{QueryResultClassName}WithCount" : QueryResultClassName;
        public string QueryParametersClassName => $"{ClassName}Parameters";

        public string QueryClassAccessor => IsInternal ? "internal" : "public"; 
        public string QueryResultClassName => QueryResultName ?? $"{ClassName}Result";
        private readonly string _QueryParameterRegex = @"--\s*@parameter[ \t]([^\n]+)[ \t]+(\S+)";
        private readonly string _QueryResultParameterRegex = @"--\s*@result[ \t]([^\n]+)[ \t]+(\S+)";
        private readonly string _QueryNamespaceParameterRegex = @"--\s*@namespace[ \t]+(\S+)";
        private readonly string _QueryUsingRegex = @"--\s*@using[ \t]+([^\n]+)";
        private readonly string _QueryIsSingleResultRegex = @"--\s*@single(?!\w)";
        private readonly string _QueryIsSingleOrDefaultResultRegex = @"--\s*@single_or_default";
        private readonly string _QueryAttributeRegex = @"--\s*@attribute[ \t]([^\n]+)\s*";
        private readonly string _QueryInternalRegex = @"--\s*@internal";
        private readonly string _QueryMapRegex = @"--\s*@map(?:[ \t]+([^\n]*?))?(?:[ \t]*->[ \t]*([^\n]+))?\s*$";
        private readonly string _QueryCountRegex = @"--\s*@count(?:[ \t]+(\S+))?";
        private readonly string _QueryResultNameRegex = @"--\s*@query_result_name[ \t]+([\S]+)";

        public QueryObjectClass(SqlFile sqlFile, ConfigOptions configOptions)
        {
            SqlFile = sqlFile;
            ConfigOptions = configOptions;

            IsSingleResult = Regex.IsMatch(sqlFile.Content, _QueryIsSingleResultRegex);
            IsSingleOrDefaultResult = Regex.IsMatch(sqlFile.Content, _QueryIsSingleOrDefaultResultRegex);
            HasMapDirective = Regex.IsMatch(sqlFile.Content, _QueryMapRegex, RegexOptions.Multiline);
            IsInternal = Regex.IsMatch(sqlFile.Content, _QueryInternalRegex);

            var queryUsingMatches = Regex.Matches(sqlFile.Content, _QueryUsingRegex);
            var queryParameterMatches = Regex.Matches(sqlFile.Content, _QueryParameterRegex);
            var queryResultParameterMatches = Regex.Matches(sqlFile.Content, _QueryResultParameterRegex);
            var queryAttributeMatches = Regex.Matches(sqlFile.Content, _QueryAttributeRegex);
            var queryMapMatches = Regex.Matches(sqlFile.Content, _QueryMapRegex, RegexOptions.Multiline);
            var queryCountMatch = Regex.Match(sqlFile.Content, _QueryCountRegex);
            var queryResultNameMatch = Regex.Match(sqlFile.Content, _QueryResultNameRegex);

            Usings = queryUsingMatches.Cast<Match>().Select(match => match.Groups[1].Value).ToArray();
            QueryParameters = queryParameterMatches
                .Cast<Match>()
                .Select(match => new ParameterType
                {
                    Type = SyntaxFactory.ParseTypeName(match.Groups[1].Value).ToString(),
                    Name = SyntaxFactory.ParseName(match.Groups[2].Value).ToString(),
                })
                .ToArray();

            ResultParameters = queryResultParameterMatches
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

            HasCount = queryCountMatch.Success;
            var countName = queryCountMatch.Groups[1].Value;
            if (HasCount && !string.IsNullOrWhiteSpace(countName))
            {
                CountName = countName;
            }
            QueryResultName = queryResultNameMatch.Success ? queryResultNameMatch.Groups[1].Value : null;
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

    private class ParameterType
    {
        public required string Name { get; set; }
        public required string Type { get; set; }
        public string PascalCaseName => Name.Pascalize();
        public string CamelCaseName => Name.Camelize();
        public string RequiredKeyword => Type.EndsWith("?") ? string.Empty : "required ";
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
        using IOKode.OpinionatedFramework.Utilities;
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

            private static readonly IReadOnlyList<string> directives = SqlUtilities.GetDirectives(Query);
            private static readonly QueryCardinality cardinality = SqlUtilities.GetCardinality(directives);

            public string RawSql => Query;

            public IReadOnlyList<string> Directives => directives;

            public QueryCardinality Cardinality => cardinality;

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

            {{~ if RequiresCustomMapResult ~}}
            private partial {{ InvokeResultString }} MapResult(IReadOnlyCollection<{{ QueryRowResultString }}> rawResults);
            {{~ else ~}}
            private {{ InvokeResultString }} MapResult(IReadOnlyCollection<{{ QueryRowResultString }}> rawResults)
            {
                {{~ if IsSingleResult ~}}
                return rawResults.First();
                {{~ else if IsSingleOrDefaultResult ~}}
                return rawResults.FirstOrDefault();
                {{~ else if HasCount ~}}
                var queryResult = new {{ QueryResultString }}
                {
                    Results = rawResults.Select(result => MapResultWithCount(result)).ToList(),
                    Count = rawResults.FirstOrDefault()?.{{ PascalCountName }} ?? 0
                };
                return queryResult;
                {{~ else ~}}
                return rawResults;
                {{~ end ~}}
            }
            {{~ end ~}}
            
            {{~ if HasCount ~}}
            private static {{ QueryResultClassName }} MapResultWithCount({{ QueryResultClassName }}WithCount resultWithCount)
            { 
                return new {{ QueryResultClassName }}
                {
                    {{~ for parameter in ResultParameters ~}}
                    {{ parameter.PascalCaseName }} = resultWithCount.{{ parameter.PascalCaseName }},
                    {{~ end ~}}
                };
            }
            {{~ end ~}}
        }

        {{ QueryClassAccessor }} partial record {{ QueryResultClassName }}
        {
            {{~ for parameter in ResultParameters ~}}
            public required {{ parameter.Type }} {{ parameter.PascalCaseName }} { get; init; }
            {{~ end ~}}
        }

        {{~ if HasCount ~}}
        {{ QueryClassAccessor }} partial record {{ QueryResultClassName }}WithCount
        {
            {{~ for parameter in ResultParameters ~}}
            public required {{ parameter.Type }} {{ parameter.PascalCaseName }} { get; init; }
            {{~ end ~}}
            public required int {{ PascalCountName }} { get; init; }
        }
        {{~ end ~}}
        
        {{ QueryClassAccessor }} partial record {{ QueryParametersClassName }}
        {
            {{~ for parameter in QueryParameters ~}}
            public {{ parameter.RequiredKeyword }}{{ parameter.Type }} {{ parameter.PascalCaseName }} { get; init; }
            {{~ end ~}}
        }
        """";
}
