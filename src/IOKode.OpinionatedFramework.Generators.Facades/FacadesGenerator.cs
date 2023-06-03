using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace IOKode.OpinionatedFramework.Generators.Facades;

[Generator]
public partial class FacadesGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if SHOULD_ATTACH_DEBUGGER
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Launch();
            }
#endif
        var facadesDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider
            (
                static (syntaxNode, _) => _IsSyntaxTargetForGeneration(syntaxNode),
                static (context, _) => _GetSemanticTargetForGeneration(context)
            )
            .Where(static declarationSyntax => declarationSyntax is not null);

        IncrementalValueProvider<(Compilation Compilation, ImmutableArray<InterfaceDeclarationSyntax> Declarations)>
            compilationAndClasses = context.CompilationProvider.Combine(facadesDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndClasses,
            static (sourceProductionContext, source) => Execute(source.Compilation, source.Declarations, sourceProductionContext));
    }
    
    /// <summary>
    /// Generate the files with the generated code and adds them to the context.
    /// </summary>
    private static void Execute(Compilation compilation, ImmutableArray<InterfaceDeclarationSyntax> interfaces,
        SourceProductionContext context)
    {
        if (interfaces.IsDefaultOrEmpty)
        {
            return;
        }

        var distinctClasses = interfaces.Distinct();
        var facades = _GetFacades(compilation, distinctClasses, context.CancellationToken)
            .ToList();

        var errors = _GetGenerationErrors(facades).ToList();
        if (errors.Count > 0)
        {
            foreach (var error in errors)
            {
                context.ReportDiagnostic(Diagnostic.Create(SignatureDuplicationDiagnostic(error.Message), error.Location));
            }

            return;
        }

        foreach (var facade in facades)
        {
            string facadeClass = _GenerateFacadeClass(facade);
            context.AddSource($"{facade.Name}.g.cs", SourceText.From(facadeClass, Encoding.UTF8));
        }
    }
    
    /// <summary>
    /// Filter interfaces at the syntactic level for code generation.
    /// </summary>
    private static bool _IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is InterfaceDeclarationSyntax interfaceDeclarationSyntax &&
               interfaceDeclarationSyntax.AttributeLists
                   .SelectMany(attributeListSyntax => attributeListSyntax.Attributes).Any();
    }

    /// <summary>
    /// Filter interfaces at the semantic level for code generation.
    /// </summary>
    private static InterfaceDeclarationSyntax _GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var interfaceDeclarationSyntax = (InterfaceDeclarationSyntax) context.Node;

        var hasAddToFacadeAttribute = interfaceDeclarationSyntax.AttributeLists
            .SelectMany(attributeListSyntax => attributeListSyntax.Attributes)
            .Select(attributeSyntax => context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol)
            .Select(attributeSymbol => attributeSymbol?.ContainingType.ToDisplayString())
            .Any(attributeFullName => attributeFullName == _AddToFacadeAttribute);
        if (!hasAddToFacadeAttribute)
        {
            return null;
        }

        return interfaceDeclarationSyntax;
    }

    private static DiagnosticDescriptor SignatureDuplicationDiagnostic(string message) => new (
        id: "OF0001",
        title: "Cannot generate facade class",
        messageFormat: message,
        category: "Source generator",
        DiagnosticSeverity.Error, isEnabledByDefault: true);
}