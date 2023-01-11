using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Scriban;
using Scriban.Runtime;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharp.CSharpExtensions;

namespace IOKode.OpinionatedFramework.Generators;

internal partial class ThrowerGenerator
{
    private record _Thrower
    {
        public string EnsurerClassName { get; set; }
        public string EnsurerClassNamespace { get; set; }
        public _ThrowerMethod[] Methods { get; set; }

        public string ClassName
        {
            get
            {
                var ensurerClassNameBuilder = new StringBuilder(EnsurerClassName);
                if (EnsurerClassName.EndsWith("Ensurer"))
                {
                    ensurerClassNameBuilder.Length -= "Ensurer".Length;
                }

                ensurerClassNameBuilder.Append("Thrower");
                return ensurerClassNameBuilder.ToString();
            }
        }
    }

    private record _ThrowerMethod
    {
        public string Name { get; set; }
        public IEnumerable<_ThrowerMethodParameter> Parameters { get; set; }
    }

    private record _ThrowerMethodParameter
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

    // private record _QueryCommandToGenerate
    // {
    //     public string QueryInterfaceName { get; set; }
    //     public string QueryInterfaceNamespace { get; set; }
    //     public IEnumerable<string> PermissionAttributes { get; set; }
    //     public string DisplayEventName { get; set; }
    //     public string QueryInterfaceType => $"{QueryInterfaceNamespace}.{QueryInterfaceName}";
    //     public string QueryName => Regex.Replace(QueryInterfaceName, _EnsurerNameRegex, "$1");
    //     public string QueryCommandName => $"{QueryName}QueryCommand";
    //     public string QueryCommandNamespace => Regex.Replace(QueryInterfaceNamespace, "[^.]+$", "QueryCommands");
    //     public string QueryCommandEventName => $"{QueryName}Queried";
    //     public string QueryCommandEventNamespace => Regex.Replace(QueryInterfaceNamespace, "[^.]+$", "Events");
    //     public string QueryCommandEventType => $"{QueryCommandEventNamespace}.{QueryCommandEventName}";
    //     public bool HasPermissionAttributes => PermissionAttributes is not null;
    // }

    private static readonly string _EnsurerAttribute = "IOKode.OpinionatedFramework.Ensuring.EnsurerAttribute";
    private static readonly string _EnsurerNameRegex = "^(.*)(Ensurer)?$"; // todo review regex
    
    /// <summary>
    /// Get the relevant information of each interface for code generation.
    /// </summary>
    private static IEnumerable<_Thrower> _GetThrowers(Compilation compilation, IEnumerable<ClassDeclarationSyntax> classes,
        CancellationToken cancellationToken)
    {
        foreach (ClassDeclarationSyntax classDeclarationSyntax in classes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var ensurerClassName = classDeclarationSyntax.Identifier.Text;
            var ensurerClassNamespace = SourceGenerationHelper.GetNamespace(classDeclarationSyntax);

            var semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);

            var methods = classDeclarationSyntax.Members
                .OfType<MethodDeclarationSyntax>()
                .Select(syntax => getMethodInfo(syntax, semanticModel))
                .Where(methodInfo => methodInfo is not null)
                .ToArray();

            var throwersToGenerate = new _Thrower
            {
                EnsurerClassName = ensurerClassName,
                EnsurerClassNamespace = ensurerClassNamespace,
                Methods = methods
            };

            yield return throwersToGenerate;
        }

        _ThrowerMethod getMethodInfo(MethodDeclarationSyntax methodDeclarationSyntax, SemanticModel semanticModel)
        {
            
            var methodSymbol = (IMethodSymbol) ModelExtensions.GetDeclaredSymbol(semanticModel, methodDeclarationSyntax)!;

            if (methodSymbol.DeclaredAccessibility != Accessibility.Public)
            {
                return null;
            }

            var boolTypeSymbol = semanticModel.Compilation.GetTypeByMetadataName(typeof(bool).FullName!); // todo should be typeSymbol?
            if (!SymbolEqualityComparer.Default.Equals(methodSymbol.ReturnType, boolTypeSymbol))
            {
                return null;
            }

            var methodName = methodDeclarationSyntax.Identifier.Text;
            var methodParameters = methodDeclarationSyntax.ParameterList.Parameters
                .Select(parameterSyntax => (IParameterSymbol) ModelExtensions.GetDeclaredSymbol(semanticModel, parameterSyntax))
                .Where(parameterSymbol => parameterSymbol is not null)
                .Select(parameterSymbol => new _ThrowerMethodParameter
                {
                    Name = parameterSymbol.Name,
                    Type = parameterSymbol.Type.ToString()
                });

            var method = new _ThrowerMethod
            {
                Name = methodName,
                Parameters = methodParameters
            };
            return method;
        }
    }

    private static string _GenerateThrowerClass(_Thrower thrower)
    {
        return Template.Parse(_GeneratedThrowerClassTemplate).Render(thrower, member => member.Name);
    }

    private static string _GenerateEnsurerHoldClass(IEnumerable<_Thrower> throwers)
    {
        var script = new ScriptObject {{ "Throwers", throwers }};
        var templateContext = new TemplateContext(script)
        {
            MemberRenamer = member => member.Name
        };
        return Template.Parse(_ThrowerHolderClassTemplate).Render(templateContext);
    }

    private static readonly string _GeneratedThrowerClassTemplate = 
        """
        // This class was auto-generated by a source generator

        using System;
        using {{ EnsurerClassNamespace }};

        namespace IOKode.OpinionatedFramework.Ensuring.Throwers;

        public class {{ ThrowerName }}
        {
            private readonly Exception _exception;

            public {{ ThrowerName }}(Exception exception)
            {
                _exception = exception;
            }

            {{~ for method in Methods ~}}
            public void {{ method.Name }}({{ for parameter in method.Parameters }}{{ parameter.Type }} {{ parameter.Name }}{{ if !for.last }}, {{ end }}{{ end }})
            {
                bool isValid = {{ EnsurerClassName }}.{{ method.Name }}({{ for parameter in method.Parameters }}{{ parameter.Name }}{{ if !for.last }}, {{ end }}{{ end }});

                if (!isValid)
                {
                    throw _exception;
                }
            }

            {{~ end ~}}
        }
        """;

    private static readonly string _ThrowerHolderClassTemplate =
        """
        // This class was auto-generated by query command source generator

        using System;
        using IOKode.OpinionatedFramework.Ensuring.Throwers;

        namespace IOKode.OpinionatedFramework.Ensuring;

        public class ThrowerHolder
        {
            private readonly Exception _exception;

            public ThrowerHolder(Exception exception)
            {
                _exception = exception;
            }

            {{~ for thrower in Throwers ~}}
            public {{ thrower.Name }}Thrower {{ thrower.Name }} => new (_exception);
            {{~ end ~}}
        }
        """;
}