using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Scriban;

namespace IOKode.OpinionatedFramework.SourceGenerators.RestResourceControllers;

/// <summary>
/// A sample source generator that creates a custom report based on class properties. The target class should be annotated with the 'Generators.ReportAttribute' attribute.
/// When using the source code as a baseline, an incremental source generator is preferable because it reduces the performance overhead.
/// </summary>
[Generator]
public partial class SourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var resourceTypesFromSyntax = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (syntaxNode, _) => syntaxNode is TypeDeclarationSyntax {AttributeLists.Count: > 0},
                transform: static (ctx, _) =>
                {
                    var declaredSymbol = (TypeDeclarationSyntax) ctx.Node;
                    return ctx.SemanticModel.GetDeclaredSymbol(declaredSymbol).ContainingAssembly;
                })
            .Where(static symbol => symbol is not null)
            .Collect(); // todo si es sencillo, lo hago

        var commandsAssembliesSet = context.AnalyzerConfigOptionsProvider.Select(GetValidCommandsAssembliesSet);
        var generatorContextData = context.CompilationProvider.Select(GetGeneratorContextData);

        var assembliesSymbols = context.MetadataReferencesProvider
            .Combine(commandsAssembliesSet)
            .Combine(context.CompilationProvider)
            .Select((tuple, _) =>
            {
                var ((reference, assembliesSet), compilation) = tuple;

                var assemblySymbol = (IAssemblySymbol) compilation.GetAssemblyOrModuleSymbol(reference)!;
                return assembliesSet.Contains(assemblySymbol.Name) ? assemblySymbol : null;
            })
            .Where(symbol => symbol != null)
            .Select((symbol, _) => symbol)
            .Collect();

        var controllersData = assembliesSymbols
            .Combine(generatorContextData)
            .Select(GetControllersData!);

        context.RegisterSourceOutput(controllersData, GenerateCode);
    }

    private static void GenerateCode(SourceProductionContext context, IEnumerable<ResourceControllerData> controllerData)
    {
        foreach (var controller in controllerData)
        {
            var controllerTemplate = Template.Parse(ControllerClassTemplate).Render(controller, member => member.Name);
            context.AddSource($"{controller.ClassFileName}", SourceText.From(controllerTemplate, Encoding.UTF8));
        }
    }

    private static IEnumerable<ResourceControllerData> GetControllersData((ImmutableArray<IAssemblySymbol>, GeneratorContextData) tuple, CancellationToken cancellationToken)
    {
        var (assemblySymbols, generatorData) = tuple;

        var commandsData = GetCommandsData(generatorData, assemblySymbols).ToList();
        var controllersData = commandsData
            .GroupBy(command => command.LowerCaseResource)
            .Select(group => new ResourceControllerData
            {
                CommandsData = group.ToArray()
            });

        return controllersData;
    }

    private static IEnumerable<CommandData> GetCommandsData(GeneratorContextData generatorData, ImmutableArray<IAssemblySymbol> assemblySymbols)
    {
        foreach (var assemblySymbol in assemblySymbols)
        {
            var typeSymbols = GetTypeSymbolsFromNamespace(assemblySymbol.GlobalNamespace);
            foreach (var typeSymbol in typeSymbols)
            {
                var commandResourceAttribute = typeSymbol
                    .GetAttributes()
                    .FirstOrDefault(attr => generatorData.ResourceTypes.ContainsKey(attr.AttributeClass!.OriginalDefinition.ToDisplayString()));
                if (commandResourceAttribute is null)
                {
                    continue;
                }

                var baseCommand = GetBaseCommand(typeSymbol, generatorData);
                if (baseCommand is null)
                {
                    continue;
                }

                var data = new CommandData
                {
                    ClassName = typeSymbol.Name,
                    Namespace = typeSymbol.ContainingNamespace.ToDisplayString(),
                    Resource = (string) commandResourceAttribute.ConstructorArguments[0].Value!,
                    GenericArgument = ((INamedTypeSymbol) baseCommand).TypeArguments.FirstOrDefault()?.ToDisplayString(),
                    ConstructorParameters = ((INamedTypeSymbol) typeSymbol).InstanceConstructors
                        .First().Parameters.Select(param => new ConstructorParameter
                        {
                            Name = param.Name,
                            Type = param.Type.ToDisplayString()
                        }).ToArray(),
                    ResourceType = generatorData.ResourceTypes[commandResourceAttribute.AttributeClass!.OriginalDefinition.ToDisplayString()],
                };

                if (commandResourceAttribute.ConstructorArguments.Length > 2)
                {
                    data.Action = commandResourceAttribute.ConstructorArguments[2].Value as string;
                }

                if (commandResourceAttribute.ConstructorArguments.Length > 1)
                {
                    data.KeyName = commandResourceAttribute.ConstructorArguments[1].Value as string;
                }

                yield return data;
            }
        }
    }

    private static ISet<string> GetValidCommandsAssembliesSet(AnalyzerConfigOptionsProvider provider, CancellationToken _)
    {
        provider.GlobalOptions.TryGetValue("build_property.CommandsDllAssemblies", out var rawValue);

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

    private static GeneratorContextData? GetGeneratorContextData(Compilation compilation, CancellationToken _)
    {
        var commandBaseSymbol = compilation.GetTypeByMetadataName("IOKode.OpinionatedFramework.Commands.Command");
        var genericCommandBaseSymbol = compilation.GetTypeByMetadataName("IOKode.OpinionatedFramework.Commands.Command`1");

        if (commandBaseSymbol is null || genericCommandBaseSymbol is null)
        {
            return null;
        }

        var generatorData = new GeneratorContextData
        {
            CommandBaseSymbol = commandBaseSymbol,
            GenericCommandBaseSymbol = genericCommandBaseSymbol
        };
        return generatorData;
    }

    private static ITypeSymbol? GetBaseCommand(ITypeSymbol derivedSymbol, GeneratorContextData generatorData)
    {
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
                    if (type is {IsAbstract: true, IsStatic: true})
                    {
                        continue;
                    }

                    yield return type;
                    break;
            }
        }
    }

    private static bool IsSubclassOf(ITypeSymbol derivedSymbol, INamedTypeSymbol baseSymbol)
    {
        var derivedSymbolParent = derivedSymbol.BaseType;
        for (var currentSymbol = derivedSymbolParent; currentSymbol != null; currentSymbol = currentSymbol.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(currentSymbol, baseSymbol))
            {
                return true;
            }
        }

        return false;
    }
}