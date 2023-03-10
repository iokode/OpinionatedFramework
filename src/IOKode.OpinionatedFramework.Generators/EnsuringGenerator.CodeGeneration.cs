using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Scriban;
using Scriban.Runtime;

namespace IOKode.OpinionatedFramework.Generators;

internal partial class EnsuringGenerator
{
    private class _Thrower
    {
        public string EnsurerClassName { get; set; }
        public string EnsurerClassNamespace { get; set; }
        public _ThrowerMethod[] Methods { get; set; }

        public string Name
        {
            get
            {
                var ensurerClassNameBuilder = new StringBuilder(EnsurerClassName);
                if (EnsurerClassName.EndsWith("Ensurer"))
                {
                    ensurerClassNameBuilder.Length -= "Ensurer".Length;
                }

                return ensurerClassNameBuilder.ToString();
            }
        }

        public string ClassName => $"{Name}Thrower";
    }

    private class _ThrowerMethod
    {
        public string Name { get; set; }
        public IEnumerable<_ThrowerMethodParameter> Parameters { get; set; }
        public string DocComment { get; set; }
    }

    private class _ThrowerMethodParameter
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

    private static readonly string _EnsurerAttribute = "IOKode.OpinionatedFramework.Ensuring.EnsurerAttribute";
    
    /// <summary>
    /// Get the relevant information of each class for code generation.
    /// </summary>
    private static IEnumerable<_Thrower> _GetThrowers(Compilation compilation, IEnumerable<ClassDeclarationSyntax> classes,
        CancellationToken cancellationToken)
    {
        foreach (var classDeclarationSyntax in classes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var ensurerClassName = classDeclarationSyntax.Identifier.Text;
            var ensurerClassNamespace = SourceGenerationHelper.GetNamespace(classDeclarationSyntax);

            var semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);
            var methods = classDeclarationSyntax.Members
                .OfType<MethodDeclarationSyntax>()
                .Select(syntax => getThrowerMethod(syntax, semanticModel))
                .Where(method => method is not null)
                .ToArray();

            var thrower = new _Thrower
            {
                EnsurerClassName = ensurerClassName,
                EnsurerClassNamespace = ensurerClassNamespace,
                Methods = methods
            };
            yield return thrower;
        }

        _ThrowerMethod getThrowerMethod(MethodDeclarationSyntax methodDeclarationSyntax, SemanticModel semanticModel)
        {
            var methodSymbol = (IMethodSymbol) semanticModel.GetDeclaredSymbol(methodDeclarationSyntax)!;
            if (methodSymbol.DeclaredAccessibility != Accessibility.Public)
            {
                return null;
            }

            var boolTypeSymbol = semanticModel.Compilation.GetTypeByMetadataName(typeof(bool).FullName!);
            if (!SymbolEqualityComparer.Default.Equals(methodSymbol.ReturnType, boolTypeSymbol))
            {
                return null;
            }

            var methodName = methodDeclarationSyntax.Identifier.Text;
            var docComment = SourceGenerationHelper.GetMethodDocComment(methodSymbol);
            var methodParameters = methodDeclarationSyntax.ParameterList.Parameters
                .Select(parameterSyntax => (IParameterSymbol) semanticModel.GetDeclaredSymbol(parameterSyntax))
                .Where(parameterSymbol => parameterSymbol is not null)
                .Select(parameterSymbol => new _ThrowerMethodParameter
                {
                    Name = parameterSymbol.Name,
                    Type = parameterSymbol.Type.ToString()
                });

            var method = new _ThrowerMethod
            {
                Name = methodName,
                Parameters = methodParameters,
                DocComment = docComment
            };
            return method;
        }
    }

    private static string _GenerateThrowerClass(_Thrower thrower)
    {
        return Template.Parse(_ThrowerClassTemplate).Render(thrower, member => member.Name);
    }

    private static string _GenerateThrowerHolderClass(IEnumerable<_Thrower> throwers)
    {
        var script = new ScriptObject {{ "Throwers", throwers }};
        var templateContext = new TemplateContext(script)
        {
            MemberRenamer = member => member.Name
        };
        return Template.Parse(_ThrowerHolderClassTemplate).Render(templateContext);
    }

    private static readonly string _ThrowerClassTemplate = 
        """
        // This file was auto-generated by a source generator

        using System;
        using {{ EnsurerClassNamespace }};

        namespace IOKode.OpinionatedFramework.Ensuring.Throwers;

        public class {{ ClassName }}
        {
            private readonly Exception _exception;

            public {{ ClassName }}(Exception exception)
            {
                _exception = exception;
            }

            {{~ for method in Methods ~}}
            {{~ if method.DocComment ~}}
            {{ method.DocComment }}
            {{~ end ~}}
            /// <exception cref="Exception">Thrown an exception when the validation no passes.</exception>
            public void {{ method.Name }}({{ for parameter in method.Parameters }}{{ parameter.Type }} {{ parameter.Name }}{{ if !for.last }}, {{ end }}{{ end }})
            {
                bool isValid = {{ EnsurerClassName }}.{{ method.Name }}({{ for parameter in method.Parameters }}{{ parameter.Name }}{{ if !for.last }}, {{ end }}{{ end }});

                if (!isValid)
                {
                    throw _exception;
                }
            }
            {{~ if !for.last ~}}

            {{~ end ~}}
            {{~ end ~}}
        }
        """;

    private static readonly string _ThrowerHolderClassTemplate =
        """
        // This file was auto-generated by a source generator

        using IOKode.OpinionatedFramework.Ensuring.Throwers;

        namespace IOKode.OpinionatedFramework.Ensuring;

        public partial class ThrowerHolder
        {
            {{~ for thrower in Throwers ~}}
            public {{ thrower.ClassName }} {{ thrower.Name }} => new (_exception);
            {{~ end ~}}
        }
        """;
}