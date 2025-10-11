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
        public string RootNamespace { get; set; }
        public string RootPath { get; set; }
    }

    private class SqlFile
    {
        public string FilePath { get; set; }
        public string Content { get; set; }
    }

    private class QueryObjectClass
    {
        public SqlFile SqlFile { get; }
        public ConfigOptions ConfigOptions { get; }
        public string Name => Path.GetFileNameWithoutExtension(SqlFile.FilePath);
        public string ClassName => Name.EndsWith("Query") ? Name : $"{Name}Query";
        public string FileName => $"{ClassName}.g.cs";

        public string Namespace => _GetNamespace();
        public string QueryContent => SqlFile.Content;
        public bool IsSingleResult => Regex.IsMatch(SqlFile.Content, _QueryIsSingleResultRegex);
        public bool IsSingleOrDefaultResult => Regex.IsMatch(SqlFile.Content, _QueryIsSingleOrDefaultResultRegex);

        public string[] Usings;
        public ParameterType[] QueryParameters;
        public ParameterType[] ResultParameters;
        public string[] Attributes;
        public string QueryResultClassName => $"{ClassName}Result";
        private readonly string _QueryParameterRegex = @"--\s*@parameter\s+(.+)\s+(\S+)\s*\n";
        private readonly string _QueryResultParameterRegex = @"--\s*@result\s+(.+)\s+(\S+)\s*\n";
        private readonly string _QueryNamespaceParameterRegex = @"--\s*@namespace\s+([\w.]+)";
        private readonly string _QueryUsingRegex = @"--\s*@using\s+([\w.]+)";
        private readonly string _QueryIsSingleResultRegex = @"--\s*@single(?!\w)";
        private readonly string _QueryIsSingleOrDefaultResultRegex = @"--\s*@single_or_default";
        private readonly string _QueryAttributeRegex = @"--\s*@attribute\s+(.+)\s*";

        public QueryObjectClass(SqlFile sqlFile, ConfigOptions configOptions)
        {
            SqlFile = sqlFile;
            ConfigOptions = configOptions;
            var queryUsingMatches = Regex.Matches(sqlFile.Content, _QueryUsingRegex);
            var queryParameterMatches = Regex.Matches(sqlFile.Content, _QueryParameterRegex);
            var queryResultParameterMatches = Regex.Matches(sqlFile.Content, _QueryResultParameterRegex);
            var queryAttributeMatches = Regex.Matches(sqlFile.Content, _QueryAttributeRegex);

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
        }

        private string _GetNamespace()
        {
            var queryNamespaceMatches = Regex.Matches(SqlFile.Content, _QueryNamespaceParameterRegex);
            var namespaceInFile = queryNamespaceMatches.Cast<Match>().FirstOrDefault()?.Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(namespaceInFile))
            {
                return namespaceInFile;
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
    }

    private class ParameterType
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string PascalCaseName => Name.Pascalize();
        public string CamelCaseName => Name.Camelize();
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
        public static partial class {{ ClassName }}
        {
            public const string Query =
                """
                {{ QueryContent }}
                """;

            public static async Task<{{ if IsSingleResult }}{{ QueryResultClassName }}{{ else if IsSingleOrDefaultResult }}{{ QueryResultClassName }}?{{ else }}IReadOnlyCollection<{{ QueryResultClassName }}>{{ end }}> InvokeAsync({{ for parameter in QueryParameters }}{{ parameter.Type }} {{ parameter.CamelCaseName }}, {{ end }}CancellationToken cancellationToken)
            {
                var queryExecutor = Locator.Resolve<IQueryExecutor>();
                var parameters = new
                {
                    {{~ for parameter in QueryParameters ~}}
                    {{ parameter.Name }} = {{ parameter.CamelCaseName }},
                    {{~ end ~}}
                };

                {{~ if IsSingleResult ~}}
                var queryResult = await queryExecutor.QuerySingleAsync<{{ QueryResultClassName }}>(Query, parameters, null, cancellationToken);
                {{~ else if IsSingleOrDefaultResult ~}}
                var queryResult = await queryExecutor.QuerySingleOrDefaultAsync<{{ QueryResultClassName }}>(Query, parameters, null, cancellationToken);
                {{~ else ~}}
                var queryResult = await queryExecutor.QueryAsync<{{ QueryResultClassName }}>(Query, parameters, null, cancellationToken);
                {{~ end ~}}
                return queryResult;
            }
        }

        public record {{ QueryResultClassName }}
        {
            {{~ for parameter in ResultParameters ~}}
            public required {{ parameter.Type }} {{ parameter.PascalCaseName }} { get; init; }
            {{~ end ~}}
        }
        """";
}
