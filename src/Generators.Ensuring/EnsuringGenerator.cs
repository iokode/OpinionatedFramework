using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace IOKode.OpinionatedFramework.Generators.Ensuring;

[Generator]
internal partial class EnsuringGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if SHOULD_ATTACH_DEBUGGER
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Launch();
            }
#endif
        var throwerDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider
            (
                static (syntaxNode, _) => _IsSyntaxTargetForGeneration(syntaxNode),
                static (context, _) => _GetSemanticTargetForGeneration(context)
            )
            .Where(static declarationSyntax => declarationSyntax is not null);

        IncrementalValueProvider<(Compilation Compilation, ImmutableArray<ClassDeclarationSyntax> Declarations)>
            compilationAndClasses =
                context.CompilationProvider.Combine(throwerDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndClasses,
            static (sourceProductionContext, source) =>
                Execute(source.Compilation, source.Declarations, sourceProductionContext));
    }

    /// <summary>
    /// Generate the files with the generated code and adds them to the context.
    /// </summary>
    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes,
        SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
        {
            return;
        }

        var distinctClasses = classes.Distinct();
        var ensurers = _GetEnsurers(compilation, distinctClasses, context.CancellationToken)
            .ToList();

        if (ensurers.Count <= 0)
        {
            return;
        }

        string ensureClass = _GenerateEnsureClass(ensurers);
        context.AddSource("Ensure.g.cs", SourceText.From(ensureClass, Encoding.UTF8));

        foreach (var ensurer in ensurers)
        {
            string ensurerThrowerClass = _GenerateEnsurerThrowerClass(ensurer);
            context.AddSource($"{ensurer.EnsurerThrowerClassName}.g.cs", SourceText.From(ensurerThrowerClass, Encoding.UTF8));
        }
    }

    /// <summary>
    /// Filter classes at the syntactic level for code generation.
    /// </summary>
    private static bool _IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax classDeclarationSyntax &&
               classDeclarationSyntax.AttributeLists
                   .SelectMany(attributeListSyntax => attributeListSyntax.Attributes).Any();
    }

    /// <summary>
    /// Filter classes at the semantic level for code generation.
    /// </summary>
    private static ClassDeclarationSyntax _GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax) context.Node;

        var classSymbol = (INamedTypeSymbol) context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax)!;
        if (!classSymbol.IsStatic)
        {
            return null;
        }

        var hasEnsurerAttribute = classDeclarationSyntax.AttributeLists
            .SelectMany(attributeListSyntax => attributeListSyntax.Attributes)
            .Select(attributeSyntax => context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol)
            .Select(attributeSymbol => attributeSymbol?.ContainingType.ToDisplayString())
            .Any(attributeFullName => attributeFullName == _EnsurerAttribute);
        if (!hasEnsurerAttribute)
        {
            return null;
        }

        return classDeclarationSyntax;
    }
}