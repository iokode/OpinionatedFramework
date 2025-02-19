﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace IOKode.OpinionatedFramework.SourceGenerators.Helpers;

internal static class SourceGenerationHelper
{
    public static string GetMethodDocComment(IMethodSymbol methodSymbol)
    {
        var docComment = methodSymbol.GetDocumentationCommentXml();
        if (!string.IsNullOrEmpty(docComment))
        {
            var strBuilder = new StringBuilder();
            var linesReader = new StringReader(docComment);
            linesReader.ReadLine();
            while (linesReader.ReadLine() is { } line)
            {
                if (linesReader.Peek() < 0)
                {
                    continue;
                }

                if (strBuilder.Length > 0)
                {
                    strBuilder.AppendLine();
                }

                strBuilder.Append("///");
                strBuilder.Append(line);
            }

            docComment = strBuilder.ToString();
        }

        return docComment;
    }

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

    public static string ToPascalCase(this string source)
    {
        return ConvertToCapitalizedCasing(source, true);
    }

    public static string ToCamelCase(this string source)
    {
        return ConvertToCapitalizedCasing(source, false);
    }

    private static string ConvertToCapitalizedCasing(string input, bool firstLetterUppercase)
    {
        var stringBuilder = new StringBuilder();
        bool shouldCapitalize = firstLetterUppercase;
        bool isFirstLetter = true;
        var currentCategory = UnicodeCategory.OtherSymbol;

        foreach (var currentCharacter in input)
        {
            var previousCategory = currentCategory;
            currentCategory = char.GetUnicodeCategory(currentCharacter); // C89l => c_89_l

            if (!isFirstLetter && previousCategory != currentCategory)
            {
                shouldCapitalize = (previousCategory, currentCategory) switch
                {
                    (UnicodeCategory.DecimalDigitNumber, UnicodeCategory.LowercaseLetter) => true,
                    (var x, UnicodeCategory.LowercaseLetter) when IsSpecialCharacter(x) => true,
                    (_, UnicodeCategory.UppercaseLetter) => true,
                    _ => false
                };
            }

            if (IsSpecialCharacter(currentCategory))
            {
                continue;
            }

            stringBuilder.Append(shouldCapitalize
                ? char.ToUpperInvariant(currentCharacter)
                : char.ToLowerInvariant(currentCharacter));
            shouldCapitalize = false;
            isFirstLetter = false;
        }

        return stringBuilder.ToString();
    }

    private static bool IsSpecialCharacter(UnicodeCategory category)
    {
        return category is not UnicodeCategory.DecimalDigitNumber
            and not UnicodeCategory.UppercaseLetter
            and not UnicodeCategory.LowercaseLetter;
    }
}