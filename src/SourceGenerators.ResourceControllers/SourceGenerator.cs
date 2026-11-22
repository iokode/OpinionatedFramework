using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Scriban;

namespace IOKode.OpinionatedFramework.SourceGenerators.ResourceControllers;

/// <summary>
/// A sample source generator that creates a custom report based on class properties. The target class should be annotated with the 'Generators.ReportAttribute' attribute.
/// When using the source code as a baseline, an incremental source generator is preferable because it reduces the performance overhead.
/// </summary>
[Generator]
public partial class SourceGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor CommandWithConstructorParameters = new(
        "OF0005",
        "Resource command receives data through its constructor",
        "Resource command '{0}' receives data through its constructor; a command receives its data as properties",
        "Resources",
        DiagnosticSeverity.Error,
        true);

    private static readonly DiagnosticDescriptor KeyWithoutProperty = new(
        "OF0006",
        "Resource key names no property",
        "Key '{0}' of resource command '{1}' names no property of the command",
        "Resources",
        DiagnosticSeverity.Error,
        true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var assembliesWithResources = context.AnalyzerConfigOptionsProvider
            .Select(GetAssembliesWithResources);
        var generatorContextData = context.CompilationProvider
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Select(GetGeneratorContextData);

        var assembliesSymbols = context.MetadataReferencesProvider
            .Combine(assembliesWithResources)
            .Combine(context.CompilationProvider)
            .Select((tuple, _) =>
            {
                var ((assemblyMetadataReference, assembliesWithCommandsSet), compilation) = tuple;

                var assemblySymbol = (IAssemblySymbol) compilation.GetAssemblyOrModuleSymbol(assemblyMetadataReference)!;
                return assembliesWithCommandsSet.Contains(assemblySymbol.Name) ? assemblySymbol : null;
            })
            .Where(symbol => symbol != null)
            .Collect();

        var controllersData = assembliesSymbols
            .Combine(generatorContextData)
            .Select(GetControllersData!);

        context.RegisterSourceOutput(controllersData, GenerateCode);
    }

    private static void GenerateCode(SourceProductionContext context, GenerationData generationData)
    {
        foreach (var diagnostic in generationData.Diagnostics)
        {
            context.ReportDiagnostic(diagnostic);
        }

        foreach (var controller in generationData.Controllers)
        {
            var controllerTemplate = Template.Parse(ControllerClassTemplate).Render(controller, member => member.Name);
            context.AddSource($"{controller.ClassFileName}", SourceText.From(controllerTemplate, Encoding.UTF8));
        }
    }
}