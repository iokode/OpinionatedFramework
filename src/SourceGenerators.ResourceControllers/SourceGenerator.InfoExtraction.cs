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
    private static GenerationData GetControllersData((ImmutableArray<IAssemblySymbol>, GeneratorContextData) tuple, CancellationToken cancellationToken)
    {
        var (assemblySymbols, generatorData) = tuple;

        assemblySymbols = [..assemblySymbols.Prepend(generatorData.CompilationAssemblySymbol)];
        var diagnostics = new List<Diagnostic>();
        var resourcesData = assemblySymbols
            .SelectMany(assemblySymbol => GetTypeSymbolsFromNamespace(assemblySymbol.GlobalNamespace))
            .Select(typeSymbol => GetResourceData(generatorData, typeSymbol, diagnostics))
            .Where(resourceData => resourceData != null)
            .Cast<ResourceData>()
            .ToArray();

        var controllersData = resourcesData
            .GroupBy(resource => (resource.GroupPrefix, Route: resource.MainResource.Pluralize().Kebaberize()))
            .Select(group => new ResourceControllerData
            {
                AssemblyNamespace = generatorData.AssemblyNamespace,
                ResourcesData = group.ToArray()
            })
            .ToArray();

        return new GenerationData
        {
            Controllers = controllersData,
            Diagnostics = diagnostics.ToArray(),
        };
    }

    private static ResourceData? GetResourceData(GeneratorContextData generatorData, ITypeSymbol typeSymbol, List<Diagnostic> diagnostics)
    {
        var resourceAttribute = typeSymbol.GetAttributes()
            .FirstOrDefault(attr => generatorData.ResourceTypes.ContainsKey(attr.AttributeClass!.OriginalDefinition.ToDisplayString()));
        if (resourceAttribute is null)
        {
            return null;
        }

        var resourceType = generatorData.ResourceTypes[resourceAttribute.AttributeClass!.OriginalDefinition.ToDisplayString()];
        var baseQuery = GetBaseQuery(typeSymbol, generatorData);
        var isQueryClass = baseQuery is not null && resourceType is ResourceType.Retrieve or ResourceType.List;
        if (isQueryClass)
        {
            var queryData = new QueryData
            {
                ResourceType = resourceType,
                ClassName = typeSymbol.Name,
                Namespace = typeSymbol.ContainingNamespace.ToDisplayString(),
                ResourceValue = GetAttributeArgument(resourceAttribute, "resource") ?? string.Empty,
                DocComment = SourceGenerationHelper.GetDocComment(typeSymbol),
                InvocationParameters = typeSymbol.GetMembers()
                    .OfType<IPropertySymbol>()
                    .Where(property => property is { IsStatic: false, DeclaredAccessibility: Accessibility.Public } && property.SetMethod is not null)
                    .Where(property => !IsSetByMiddleware(property))
                    .Select(property => new Parameter
                    {
                        Name = property.Name.Camelize(),
                        PropertyName = property.Name,
                        Type = property.Type.ToDisplayString()
                    }).ToArray(),
                MethodReturnType = $"Task<{((INamedTypeSymbol)baseQuery!).TypeArguments.First().ToDisplayString()}>",
                KeyValue = GetAttributeArgument(resourceAttribute, "key"),
                Group = GetAttributeArgument(resourceAttribute, "group"),
            };
            return queryData;
        }

        var baseCommand = GetBaseCommand(typeSymbol, generatorData);
        var isBaseCommand = baseCommand is not null;
        if (isBaseCommand)
        {
            var location = typeSymbol.Locations.FirstOrDefault() ?? Location.None;
            if (((INamedTypeSymbol) typeSymbol).InstanceConstructors.Any(constructor => constructor.Parameters.Length > 0))
            {
                diagnostics.Add(Diagnostic.Create(CommandWithConstructorParameters, location, typeSymbol.ToDisplayString()));
                return null;
            }

            var commandData = new CommandData
            {
                ResourceType = resourceType,
                ClassName = typeSymbol.Name,
                Namespace = typeSymbol.ContainingNamespace.ToDisplayString(),
                ResourceValue = GetAttributeArgument(resourceAttribute, "resource") ?? string.Empty,
                DocComment = SourceGenerationHelper.GetDocComment(typeSymbol),
                GenericArgument = ((INamedTypeSymbol) baseCommand!).TypeArguments.FirstOrDefault()?.ToDisplayString(),
                InvocationParameters = GetCommandDataProperties(typeSymbol, generatorData)
                    .Select(property => new Parameter
                    {
                        Name = property.Name.Camelize(),
                        PropertyName = property.Name,
                        Type = property.Type.ToDisplayString(),
                        IsRequired = property.IsRequired
                    }).ToArray(),
                DeclaringTypesByProperty = GetCommandDataProperties(typeSymbol, generatorData)
                    .ToDictionary(property => property.Name, property => property.ContainingType.ToDisplayString()),
                KeyValue = GetAttributeArgument(resourceAttribute, "key"),
                Action = GetAttributeArgument(resourceAttribute, "action"),
                Group = GetAttributeArgument(resourceAttribute, "group"),
            };

            var unknownKeys = commandData.KeysByResource.Values
                .SelectMany(keys => keys)
                .Where(key => commandData.InvocationParameters.All(parameter => parameter.Name != key))
                .ToArray();
            foreach (var unknownKey in unknownKeys)
            {
                diagnostics.Add(Diagnostic.Create(KeyWithoutProperty, location, unknownKey, typeSymbol.ToDisplayString()));
            }

            return unknownKeys.Length == 0 ? commandData : null;
        }

        return null;
    }

    /// <summary>
    /// The data of a command: its public instance properties with a setter, its own and those of its base classes.
    /// </summary>
    private static IEnumerable<IPropertySymbol> GetCommandDataProperties(ITypeSymbol typeSymbol, GeneratorContextData generatorData)
    {
        var types = new List<ITypeSymbol>();
        for (var current = typeSymbol; current != null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, generatorData.CommandBaseSymbol)
                || SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, generatorData.GenericCommandBaseSymbol))
            {
                break;
            }

            types.Insert(0, current);
        }

        return types
            .SelectMany(type => type.GetMembers().OfType<IPropertySymbol>())
            .Where(property => property is { IsStatic: false, IsIndexer: false, DeclaredAccessibility: Accessibility.Public }
                               && property.SetMethod is { DeclaredAccessibility: Accessibility.Public }
                               && !property.IsOverride);
    }

    /// <summary>
    /// Gets the value of a constructor argument of a resource attribute by the name of its parameter, so that adding
    /// an optional parameter to an attribute does not shift the position of the others.
    /// </summary>
    private static string? GetAttributeArgument(AttributeData attribute, string parameterName)
    {
        var parameters = attribute.AttributeConstructor?.Parameters ?? ImmutableArray<IParameterSymbol>.Empty;
        for (var i = 0; i < parameters.Length && i < attribute.ConstructorArguments.Length; i++)
        {
            if (parameters[i].Name == parameterName)
            {
                return attribute.ConstructorArguments[i].Value as string;
            }
        }

        return null;
    }

    /// <summary>
    /// Whether a property of a query object is marked as set by a query middleware, so it is not bound from the request.
    /// </summary>
    private static bool IsSetByMiddleware(IPropertySymbol property)
    {
        return property.GetAttributes().Any(attribute =>
            attribute.AttributeClass?.ToDisplayString() == "IOKode.OpinionatedFramework.Persistence.Queries.SetByMiddlewareAttribute");
    }

    private static ISet<string> GetAssembliesWithResources(AnalyzerConfigOptionsProvider provider, CancellationToken _)
    {
        provider.GlobalOptions.TryGetValue("build_property.ResourcesDllAssemblies", out var rawValue);

        var set = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return set;
        }

        foreach (var token in rawValue!.Split([';', ','], StringSplitOptions.RemoveEmptyEntries))
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
        var genericQuerySymbol = compilation.GetTypeByMetadataName("IOKode.OpinionatedFramework.Persistence.Queries.IQuery`1");

        if (commandBaseSymbol is null || genericCommandBaseSymbol is null || genericQuerySymbol is null)
        {
            return null;
        }

        var generatorData = new GeneratorContextData
        {
            CommandBaseSymbol = commandBaseSymbol,
            GenericCommandBaseSymbol = genericCommandBaseSymbol,
            GenericQuerySymbol = genericQuerySymbol,
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

    private static ITypeSymbol? GetBaseQuery(ITypeSymbol typeSymbol, GeneratorContextData generatorData)
    {
        if (typeSymbol.IsStatic)
        {
            return null;
        }

        return typeSymbol.AllInterfaces.FirstOrDefault(currentSymbol =>
            SymbolEqualityComparer.Default.Equals(currentSymbol.OriginalDefinition, generatorData.GenericQuerySymbol));
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
