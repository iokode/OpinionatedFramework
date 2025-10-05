using System;
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

    /// <summary>
    /// Converts a string to kebab-case, with words separated by hyphens.
    /// </summary>
    /// <param name="text">The input string to be converted to kebab-case.</param>
    /// <returns>A kebab-case representation of the input string.</returns>
    /// <remarks>
    /// Method is taken from the CaseConverter library by Mark Castle.
    /// github.com/markcastle/CaseConverter
    /// </remarks>
    public static string ToKebabCase(this string text)
    {
        // Return the input text if it's null or empty
        if (string.IsNullOrEmpty(text)) return text;

        // Initialize a StringBuilder to store the result
        StringBuilder result = new();
        // Define a flag to track whether the previous character is a separator
        bool previousCharacterIsSeparator = true;

        // Iterate through each character in the input text
        for (int i = 0; i < text.Length; i++)
        {
            char currentChar = text[i];

            // If the current character is an uppercase letter or a digit
            if (char.IsUpper(currentChar) || char.IsDigit(currentChar))
            {
                // Add a hyphen if the previous character is not a separator and
                // the current character is preceded by a lowercase letter or followed by a lowercase letter
                if (!previousCharacterIsSeparator && (i > 0 && (char.IsLower(text[i - 1]) || (i < text.Length - 1 && char.IsLower(text[i + 1])))))
                {
                    result.Append("-");
                }

                // Append the lowercase version of the current character to the result
                result.Append(char.ToLowerInvariant(currentChar));
                // Update the flag to indicate that the current character is not a separator
                previousCharacterIsSeparator = false;
            }
            // If the current character is a lowercase letter
            else if (char.IsLower(currentChar))
            {
                // Append the current character to the result
                result.Append(currentChar);
                // Update the flag to indicate that the current character is not a separator
                previousCharacterIsSeparator = false;
            }
            // If the current character is a space, underscore, or hyphen
            else if (currentChar == ' ' || currentChar == '_' || currentChar == '-')
            {
                // Add a hyphen if the previous character is not a separator
                if (!previousCharacterIsSeparator)
                {
                    result.Append("-");
                }

                // Update the flag to indicate that the current character is a separator
                previousCharacterIsSeparator = true;
            }
        }

        // Return the kebab-case representation of the input string
        return result.ToString();
    }

    private static bool IsSpecialCharacter(UnicodeCategory category)
    {
        return category is not UnicodeCategory.DecimalDigitNumber
            and not UnicodeCategory.UppercaseLetter
            and not UnicodeCategory.LowercaseLetter;
    }
}