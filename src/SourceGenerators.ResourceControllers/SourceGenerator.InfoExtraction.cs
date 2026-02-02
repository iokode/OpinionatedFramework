using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Humanizer;
using IOKode.OpinionatedFramework.SourceGenerators.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace IOKode.OpinionatedFramework.SourceGenerators.ResourceControllers;

public partial class SourceGenerator
{
    private static IEnumerable<ResourceControllerData> GetControllersData((ImmutableArray<IAssemblySymbol>, GeneratorContextData) tuple, CancellationToken cancellationToken)
    {
        var (assemblySymbols, generatorData) = tuple;

        assemblySymbols = assemblySymbols.Prepend(generatorData.CompilationAssemblySymbol).ToImmutableArray();
        var resourcesData = assemblySymbols
            .SelectMany(assemblySymbol => GetTypeSymbolsFromNamespace(assemblySymbol.GlobalNamespace))
            .Select(typeSymbol => GetResourceData(generatorData, typeSymbol))
            .Where(resourceData => resourceData != null)
            .Cast<ResourceData>();

        var controllersData = resourcesData
            .GroupBy(command => command.Resource.Pluralize().Kebaberize())
            .Select(group => new ResourceControllerData
            {
                AssemblyNamespace = generatorData.AssemblyNamespace,
                ResourcesData = group.ToArray()
            });

        return controllersData;
    }

    private static ResourceData? GetResourceData(GeneratorContextData generatorData, ITypeSymbol typeSymbol)
    {
        var resourceAttribute = typeSymbol.GetAttributes()
            .FirstOrDefault(attr => generatorData.ResourceTypes.ContainsKey(attr.AttributeClass!.OriginalDefinition.ToDisplayString()));
        if (resourceAttribute is null)
        {
            return null;
        }

        var resourceType = generatorData.ResourceTypes[resourceAttribute.AttributeClass!.OriginalDefinition.ToDisplayString()];
        var isQueryClass = typeSymbol is {IsStatic: true} && typeSymbol.Name.EndsWith("Query") && resourceType is ResourceType.Retrieve or ResourceType.List;
        if (isQueryClass)
        {
            var queryData = new QueryData
            {
                ResourceType = resourceType,
                ClassName = typeSymbol.Name,
                Namespace = typeSymbol.ContainingNamespace.ToDisplayString(),
                Resource = (string) resourceAttribute.ConstructorArguments[0].Value!,
                DocComment = SourceGenerationHelper.GetDocComment(typeSymbol),
                InvocationParameters = ((IMethodSymbol) typeSymbol.GetMembers("InvokeAsync").Single())
                    .Parameters.Select(param => new Parameter
                    {
                        Name = param.Name,
                        Type = param.Type.ToDisplayString()
                    }).ToArray(),
                MethodReturnType = ((IMethodSymbol) typeSymbol.GetMembers("InvokeAsync").Single()).ReturnType.ToDisplayString(),
                KeyName = resourceAttribute.ConstructorArguments.Length > 1 ? resourceAttribute.ConstructorArguments[1].Value as string : null,
            };
            return queryData;
        }

        var baseCommand = GetBaseCommand(typeSymbol, generatorData);
        var isBaseCommand = baseCommand is not null;
        if (isBaseCommand)
        {
            var commandData = new CommandData
            {
                ResourceType = resourceType,
                ClassName = typeSymbol.Name,
                Namespace = typeSymbol.ContainingNamespace.ToDisplayString(),
                Resource = (string) resourceAttribute.ConstructorArguments[0].Value!,
                DocComment = SourceGenerationHelper.GetDocComment(typeSymbol),
                GenericArgument = ((INamedTypeSymbol) baseCommand!).TypeArguments.FirstOrDefault()?.ToDisplayString(),
                InvocationParameters = ((INamedTypeSymbol) typeSymbol).InstanceConstructors
                    .First().Parameters
                    .Select(param => new Parameter
                    {
                        Name = param.Name,
                        Type = param.Type.ToDisplayString()
                    }).ToArray(),
                KeyName = resourceAttribute.ConstructorArguments.Length switch
                {
                    > 2 => resourceAttribute.ConstructorArguments[2].Value as string,
                    > 1 => resourceAttribute.ConstructorArguments[1].Value as string,
                    _ => null,
                },
                Action = resourceAttribute.ConstructorArguments.Length > 2
                    ? resourceAttribute.ConstructorArguments[1].Value as string
                    : null,
            };
            return commandData;
        }

        return null;
    }

    private static ISet<string> GetAssembliesWithResources(AnalyzerConfigOptionsProvider provider, CancellationToken _)
    {
        provider.GlobalOptions.TryGetValue("build_property.ResourcesDllAssemblies", out var rawValue);

        var set = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return set;
        }

        foreach (var token in rawValue!.Split(new[] {';', ','}, StringSplitOptions.RemoveEmptyEntries))
        {
            var name = token.Trim();
            if (name.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
            {
                name = name.Substring(0, name.Length - 4);
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                set.Add(name);
            }
        }

        return set;
    }

    private static GeneratorContextData? GetGeneratorContextData((Compilation, AnalyzerConfigOptionsProvider) tuple, CancellationToken _)
    {
        var (compilation, configOptionsProvider) = tuple;
        configOptionsProvider.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace);
        var commandBaseSymbol = compilation.GetTypeByMetadataName("IOKode.OpinionatedFramework.Commands.Command");
        var genericCommandBaseSymbol = compilation.GetTypeByMetadataName("IOKode.OpinionatedFramework.Commands.Command`1");

        if (commandBaseSymbol is null || genericCommandBaseSymbol is null)
        {
            return null;
        }

        var generatorData = new GeneratorContextData
        {
            CommandBaseSymbol = commandBaseSymbol,
            GenericCommandBaseSymbol = genericCommandBaseSymbol,
            CompilationAssemblySymbol = compilation.Assembly,
            AssemblyNamespace = rootNamespace ?? compilation.AssemblyName ?? compilation.GlobalNamespace.Name,
        };
        return generatorData;
    }

    private static ITypeSymbol? GetBaseCommand(ITypeSymbol derivedSymbol, GeneratorContextData generatorData)
    {
        if (derivedSymbol.IsStatic)
        {
            return null;
        }

        var derivedSymbolParent = derivedSymbol.BaseType;
        for (var currentSymbol = derivedSymbolParent; currentSymbol != null; currentSymbol = currentSymbol.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(currentSymbol, generatorData.CommandBaseSymbol))
            {
                return generatorData.CommandBaseSymbol;
            }

            if (SymbolEqualityComparer.Default.Equals(currentSymbol.OriginalDefinition, generatorData.GenericCommandBaseSymbol))
            {
                return currentSymbol;
            }
        }

        return null;
    }

    private static IEnumerable<ITypeSymbol> GetTypeSymbolsFromNamespace(INamespaceSymbol @namespace)
    {
        foreach (var namespaceOrTypeSymbol in @namespace.GetMembers())
        {
            switch (namespaceOrTypeSymbol)
            {
                case INamespaceSymbol namespaceSymbol:
                {
                    foreach (var nestedSymbol in GetTypeSymbolsFromNamespace(namespaceSymbol))
                    {
                        yield return nestedSymbol;
                    }

                    break;
                }

                case ITypeSymbol type:
                {
                    if (type is { TypeKind: TypeKind.Class, IsAbstract: true })
                    {
                        continue;
                    }

                    yield return type;
                    break;
                }
            }
        }
    }
}