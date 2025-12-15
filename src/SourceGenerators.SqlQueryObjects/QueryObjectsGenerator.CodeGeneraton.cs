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
        public bool IsAbstract { get; private set; }
        public bool IsInternal { get; private set; }

        public string[] Usings { get; private set; }
        public ParameterType[] QueryParameters { get; private set; }
        public ParameterType[] ResultParameters { get; private set;}
        public string[] Attributes { get; private set; }

        public string? AbstractQueryParametersString
        {
            get;
            set => field = string.IsNullOrWhiteSpace(value) || value!.Contains("CancellationToken") ? value : value + ", CancellationToken cancellationToken";
        }
        public string? AbstractQueryResultString { get; set; }

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
                : $"IReadOnlyCollection<{QueryResultClassName}>";
        
        public string InvokeParametersString => IsAbstract && !string.IsNullOrWhiteSpace(AbstractQueryParametersString)
            ? AbstractQueryParametersString!
            : QueryParametersString;

        public string InvokeResultString => IsAbstract && !string.IsNullOrWhiteSpace(AbstractQueryResultString)
            ? AbstractQueryResultString!
            : QueryResultString;

        public string QueryClassAccessor => IsInternal ? "internal" : "public"; 
        public string QueryResultClassName => $"{ClassName}Result";
        private readonly string _QueryParameterRegex = @"--\s*@parameter\s+(.+?)\s+(\S+)\s*";
        private readonly string _QueryResultParameterRegex = @"--\s*@result\s+(.+?)\s+(\S+)\s*";
        private readonly string _QueryNamespaceParameterRegex = @"--\s*@namespace\s+([\w.]+)";
        private readonly string _QueryUsingRegex = @"--\s*@using\s+([\w.]+)";
        private readonly string _QueryIsSingleResultRegex = @"--\s*@single(?!\w)";
        private readonly string _QueryIsSingleOrDefaultResultRegex = @"--\s*@single_or_default";
        private readonly string _QueryAttributeRegex = @"--\s*@attribute\s+([^\n]+)\s*";
        private readonly string _QueryInternalRegex = @"--\s*@internal\s*";
        private readonly string _QueryAbstractRegex = @"--\s*@abstract(?:[ \t]+([^->\n]+))?(?:[ \t]*->[ \t]*([^\n]+))?";

        public QueryObjectClass(SqlFile sqlFile, ConfigOptions configOptions)
        {
            SqlFile = sqlFile;
            ConfigOptions = configOptions;

            IsSingleResult = Regex.IsMatch(sqlFile.Content, _QueryIsSingleResultRegex);
            IsSingleOrDefaultResult = Regex.IsMatch(sqlFile.Content, _QueryIsSingleOrDefaultResultRegex);
            IsAbstract = Regex.IsMatch(sqlFile.Content, _QueryAbstractRegex);
            IsInternal = Regex.IsMatch(sqlFile.Content, _QueryInternalRegex);

            var queryUsingMatches = Regex.Matches(sqlFile.Content, _QueryUsingRegex);
            var queryParameterMatches = Regex.Matches(sqlFile.Content, _QueryParameterRegex);
            var queryResultParameterMatches = Regex.Matches(sqlFile.Content, _QueryResultParameterRegex);
            var queryAttributeMatches = Regex.Matches(sqlFile.Content, _QueryAttributeRegex);
            var queryAbstractMatches = Regex.Matches(sqlFile.Content, _QueryAbstractRegex);

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

            AbstractQueryParametersString = queryAbstractMatches.Cast<Match>().FirstOrDefault()?.Groups[1].Value;
            AbstractQueryResultString = queryAbstractMatches.Cast<Match>().FirstOrDefault()?.Groups[2].Value;

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
    }

    private class ParameterType
    {
        public required string Name { get; set; }
        public required string Type { get; set; }
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
        {{ QueryClassAccessor }} static partial class {{ ClassName }}
        {
            public const string Query =
                """
                {{ QueryContent }}
                """;

            private static async Task<{{ QueryResultString }}> QueryAsync({{ QueryParametersString }})
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

            {{~ if IsAbstract ~}}
            public static partial Task<{{ InvokeResultString }}> InvokeAsync({{ InvokeParametersString }});
            {{~ else ~}}
            public static async Task<{{ InvokeResultString }}> InvokeAsync({{ InvokeParametersString }})
            {
                return await QueryAsync({{ for parameter in QueryParameters }}{{ parameter.CamelCaseName }}, {{ end }}cancellationToken);
            }
            {{~ end ~}}
        }

        public partial record {{ QueryResultClassName }}
        {
            {{~ for parameter in ResultParameters ~}}
            public required {{ parameter.Type }} {{ parameter.PascalCaseName }} { get; init; }
            {{~ end ~}}
        }
        """";
}
