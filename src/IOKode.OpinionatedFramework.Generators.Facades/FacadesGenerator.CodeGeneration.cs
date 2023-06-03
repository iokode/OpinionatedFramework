using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Scriban;
using Scriban.Runtime;

namespace IOKode.OpinionatedFramework.Generators.Facades;

public partial class FacadesGenerator
{
    private class _Facade
    {
        public string Name { get; set; }
        public _FacadeMethod[] Methods { get; set; }
    }

    private class _FacadeMethod
    {
        public string Name { get; set; }
        public string ReturnType { get; set; }
        public IEnumerable<_FacadeMethodParameter> Parameters { get; set; }
        public IEnumerable<_FacadeMethodGenericParameter> GenericTypeParameters { get; set; }
        public string DocComment { get; set; }
        public string ContractFullName { get; set; }
        public string FacadeName { get; set; }
        public IMethodSymbol MethodSymbol { get; set; }
        public bool Has => !string.IsNullOrEmpty(DocComment);
    }

    private class _FacadeMethodParameter
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

    private class _FacadeMethodGenericParameter
    {
        public string Name { get; set; }
        public string Constraints { get; set; }
        public bool HasConstraints => Constraints.Length > 0;
    }

    private static readonly string _AddToFacadeAttribute = "IOKode.OpinionatedFramework.Contracts.AddToFacadeAttribute";

    /// <summary>
    /// Get the relevant information of each class for code generation.
    /// </summary>
    private static IEnumerable<_Facade> _GetFacades(Compilation compilation,
        IEnumerable<InterfaceDeclarationSyntax> interfaces,
        CancellationToken cancellationToken)
    {
        var facadeMethods = getFacadeMethods();
        var facades = facadeMethods.GroupBy(method => method.FacadeName)
            .Select(methodsGroup => new _Facade
            {
                Name = methodsGroup.Key,
                Methods = methodsGroup.ToArray()
            });
        return facades;

        IEnumerable<_FacadeMethod> getFacadeMethods()
        {
            foreach (var interfaceDeclarationSyntax in interfaces)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var semanticModel = compilation.GetSemanticModel(interfaceDeclarationSyntax.SyntaxTree);
                var interfaceSymbol = (INamedTypeSymbol)semanticModel.GetDeclaredSymbol(interfaceDeclarationSyntax)!;
                var interfaceFullName = interfaceSymbol.ToString();
                var facadeName = (string)interfaceSymbol!
                    .GetAttributes()
                    .Single(attribute => attribute.AttributeClass!.ToDisplayString() == _AddToFacadeAttribute)
                    .ConstructorArguments
                    .Single()
                    .Value;

                foreach (var methodDeclarationSyntax in interfaceDeclarationSyntax.Members
                             .OfType<MethodDeclarationSyntax>())
                {
                    var methodSymbol = (IMethodSymbol)semanticModel.GetDeclaredSymbol(methodDeclarationSyntax)!;
                    if (methodSymbol.DeclaredAccessibility != Accessibility.Public)
                    {
                        continue;
                    }

                    var methodName = methodDeclarationSyntax.Identifier.Text;
                    var methodReturnType = methodSymbol?.ReturnType.ToDisplayString();
                    var docComment = SourceGenerationHelper.GetMethodDocComment(methodSymbol);
                    var methodParameters = methodDeclarationSyntax.ParameterList.Parameters
                        .Select(parameterSyntax => (IParameterSymbol)semanticModel.GetDeclaredSymbol(parameterSyntax))
                        .Where(parameterSymbol => parameterSymbol is not null)
                        .Select(parameterSymbol => new _FacadeMethodParameter
                        {
                            Name = parameterSymbol.Name,
                            Type = parameterSymbol.Type.ToString()
                        });

                    var methodGenericTypeParameters = methodSymbol.TypeParameters.Select(typeParam =>
                        new _FacadeMethodGenericParameter()
                        {
                            Name = typeParam.Name,
                            Constraints = string.Join(", ",
                                typeParam.ConstraintTypes.Select(constraint => constraint.ToDisplayString()))
                        });

                    var method = new _FacadeMethod
                    {
                        Name = methodName,
                        ReturnType = methodReturnType,
                        Parameters = methodParameters,
                        GenericTypeParameters = methodGenericTypeParameters,
                        DocComment = docComment,
                        FacadeName = facadeName,
                        ContractFullName = interfaceFullName,
                        MethodSymbol = methodSymbol,
                    };
                    yield return method;
                }
            }
        }
    }

    private static IEnumerable<(string Message, Location Location)> _GetGenerationErrors(IEnumerable<_Facade> facades)
    {
        foreach (var facade in facades)
        {
            var methodsWithSameName = facade.Methods.Where(method => method.Name == facade.Name);
            foreach (var methodWithSameName in methodsWithSameName)
            {
                yield return
                (
                    $"The method name '{methodWithSameName.Name}' cannot be the same as the name of its facade.",
                    methodWithSameName.MethodSymbol.Locations.FirstOrDefault()
                );
            }

            var overloadedMethodsGroupedByName = facade.Methods
                .GroupBy(method => method.Name)
                .Where(group => group.Count() > 1);
            foreach (var methodsGroup in overloadedMethodsGroupedByName)
            {
                var overloadedMethods = methodsGroup.ToList();

                for (var i = 0; i < overloadedMethods.Count - 1; i++)
                {
                    var definedMethod = overloadedMethods[i];
                    var definedMethodParameters = definedMethod.MethodSymbol.Parameters
                        .Select(parameter => parameter.Type)
                        .ToArray();

                    for (var j = i + 1; j < overloadedMethods.Count; j++)
                    {
                        var overloadedMethod = overloadedMethods[j];
                        var overloadedMethodParameters = overloadedMethod.MethodSymbol.Parameters
                            .Select(parameter => parameter.Type)
                            .ToArray();

                        if (definedMethodParameters.Length != overloadedMethodParameters.Length)
                        {
                            continue;
                        }

                        var isSameSignature = definedMethodParameters
                            .Zip(overloadedMethodParameters,
                                (firstDefinedMethodArgumentType, overloadedMethodArgumentType) =>
                                    (firstDefinedMethodArgumentType, overloadedMethodArgumentType))
                            .All(arguments =>
                                SymbolEqualityComparer.Default.Equals(arguments.firstDefinedMethodArgumentType,
                                    arguments.overloadedMethodArgumentType));

                        if (isSameSignature)
                        {
                            yield return
                            (
                                $"The facade '{definedMethod.FacadeName}' already has a method '{overloadedMethod.Name}' with the same signature from '{definedMethod.MethodSymbol.ContainingType}'.",
                                overloadedMethod.MethodSymbol.Locations.FirstOrDefault()
                            );
                        }
                    }
                }
            }
        }
    }

    private static string _GenerateFacadeClass(_Facade facade)
    {
        var script = new ScriptObject();
        script.Import(facade, renamer: member => member.Name);
        script.Import("has_constraints", new Func<IEnumerable<_FacadeMethodGenericParameter>, bool>(_HasConstraints));

        var context = new TemplateContext();
        context.MemberRenamer = member => member.Name;

        context.PushGlobal(script);

        return Template.Parse(_FacadeClassTemplate)
            .Render(context);
    }

    private static bool _HasConstraints(IEnumerable<_FacadeMethodGenericParameter> genericTypeParameters)
    {
        if (genericTypeParameters == null)
        {
            return false;
        }
        
        return genericTypeParameters.Any(parameter => !string.IsNullOrEmpty(parameter.Constraints));
    }

    private static readonly string _FacadeClassTemplate =
        """
        // This file was auto-generated by a source generator

        using IOKode.OpinionatedFramework;

        namespace IOKode.OpinionatedFramework.Facades;

        public static class {{ Name }}
        {
            {{~ for method in Methods ~}}
            {{~ if method.Has ~}}
            {{ method.DocComment }}
            {{~ end ~}}
            public static {{ method.ReturnType }} {{ method.Name }}{{ if !method.GenericTypeParameters.empty? }}<{{ for parameter in method.GenericTypeParameters }}{{ parameter.Name }}{{ if !for.last }}, {{ end }}{{ end }}>{{ end }}({{ for parameter in method.Parameters }}{{ parameter.Type }} {{ parameter.Name }}{{ if !for.last }}, {{ end }}{{ end }}) {{ for parameter in method.GenericTypeParameters }}{{ if parameter.HasConstraints }}where {{ parameter.Name }} : {{ parameter.Constraints }}{{ end }}{{ end }}
            {
                return Locator.Resolve<{{ method.ContractFullName }}>().{{ method.Name }}{{~ if !method.GenericTypeParameters.empty? ~}}<{{~ for param in method.GenericTypeParameters ~}}{{ param.Name }}{{~ if !for.last ~}}, {{~ end ~}}{{~ end ~}}>{{~ end ~}}({{~ for param in method.Parameters ~}}{{ param.Name }}{{~ if !for.last ~}}, {{~ end ~}}{{~ end ~}});
            }
            {{~ if !for.last ~}}

            {{~ end ~}}
            {{~ end ~}}
        }
        """;
}