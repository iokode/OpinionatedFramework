using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace IOKode.OpinionatedFramework.Generators;

internal static class SourceGenerationHelper
{
    public static readonly NamespacesComparerClass NamespacesComparer = new();

    public static string GetNamespace(BaseTypeDeclarationSyntax syntax)
    {
        string nameSpace = string.Empty;

        SyntaxNode potentialNamespaceParent = syntax.Parent;
    
        while (potentialNamespaceParent is not null &&
               potentialNamespaceParent is not NamespaceDeclarationSyntax &&
               potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }

        if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
        {
            nameSpace = namespaceParent.Name.ToString();
        
            while (namespaceParent.Parent is NamespaceDeclarationSyntax parent)
            {
                nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                namespaceParent = parent;
            }
        }

        return nameSpace;
    }

    public static IEnumerable<string> GetUsingDirectives(BaseTypeDeclarationSyntax syntax)
    {
        var rootSyntaxNode = syntax.Parent;
        while (rootSyntaxNode is not null && 
               rootSyntaxNode is not CompilationUnitSyntax)
        {
            rootSyntaxNode = rootSyntaxNode.Parent;
        }

        var usingDirectives = Enumerable.Empty<string>();
        if (rootSyntaxNode is CompilationUnitSyntax compilationUnit)
        {
            usingDirectives = compilationUnit.Usings.Select(directive => directive.Name.ToString());
        }

        return usingDirectives;
    }

    public static IEnumerable<string> GetNamespacesFromTypeSyntax(SemanticModel semanticModel, params SyntaxNode[] syntaxNodes)
    {
        return syntaxNodes
            .SelectMany(syntaxNode => syntaxNode
                .DescendantNodesAndSelf()
                .OfType<TypeSyntax>()
                .Select(typeSyntax => semanticModel
                    .GetSymbolInfo(typeSyntax)
                    .Symbol!
                    .ContainingNamespace
                    .ToDisplayString()))
            .OrderBy(@namespace => @namespace, NamespacesComparer)
            .Distinct();
    }

    public class NamespacesComparerClass : Comparer<string>
    {
        public override int Compare(string namespace1, string namespace2)
        {
            var namespace1HasSystem = namespace1?.StartsWith(nameof(System)) ?? false;
            var namespace2HasSystem = namespace2?.StartsWith(nameof(System)) ?? false;

            return (namespace1HasSystem, namespace2HasSystem) switch
            {
                (true, true) => string.Compare(namespace1, namespace2, StringComparison.Ordinal),
                (true, false) => -1,
                (false, true) => 1,
                (false, false) => string.Compare(namespace1, namespace2, StringComparison.Ordinal)
            };
        }
    }
}