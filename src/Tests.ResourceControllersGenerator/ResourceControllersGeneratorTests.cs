using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using IOKode.OpinionatedFramework.Commands;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.ResourceControllersGenerator;

public class ResourceControllersGeneratorTests
{
    private static readonly ImmutableArray<MetadataReference> CompilationReferences = CreateCompilationReferences();

    [Fact]
    public void CommandWithItsDataAsProperties_HasNoDiagnostics()
    {
        // Act
        var runResult = RunGenerator("""
            [UpdateResource("user", "code")]
            public class UpdateUserCommand : Command
            {
                public required int Code { get; init; }
                public required string Name { get; init; }

                protected override Task ExecuteAsync(ICommandExecutionContext executionContext) => Task.CompletedTask;
            }
            """);

        // Assert
        Assert.Empty(runResult.Diagnostics);
        var generatedSource = Assert.Single(Assert.Single(runResult.Results).GeneratedSources).SourceText.ToString();
        Assert.Contains("[FromRoute] int code, [FromCommandBody(\"Code\")] Consumer.UpdateUserCommand? commandBody", generatedSource);
    }

    [Fact]
    public void CommandWithConstructorParameters_ProducesDiagnostic()
    {
        // Act
        var runResult = RunGenerator("""
            [UpdateResource("user", "code")]
            public class UpdateUserCommand(int code) : Command
            {
                protected override Task ExecuteAsync(ICommandExecutionContext executionContext) => Task.CompletedTask;
            }
            """);

        // Assert
        var diagnostic = Assert.Single(runResult.Diagnostics);
        Assert.Equal("OF0005", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal(
            "Resource command 'Consumer.UpdateUserCommand' receives data through its constructor; a command receives its data as properties",
            diagnostic.GetMessage());
    }

    [Fact]
    public void KeyThatNamesNoProperty_ProducesDiagnostic()
    {
        // Act
        var runResult = RunGenerator("""
            [UpdateResource("user", "code")]
            public class UpdateUserCommand : Command
            {
                public required int Id { get; init; }

                protected override Task ExecuteAsync(ICommandExecutionContext executionContext) => Task.CompletedTask;
            }
            """);

        // Assert
        var diagnostic = Assert.Single(runResult.Diagnostics);
        Assert.Equal("OF0006", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal("Key 'code' of resource command 'Consumer.UpdateUserCommand' names no property of the command", diagnostic.GetMessage());
    }

    private static GeneratorDriverRunResult RunGenerator(string commandSource)
    {
        var source = $$"""
            using System.Threading.Tasks;
            using IOKode.OpinionatedFramework.Commands;
            using IOKode.OpinionatedFramework.Resources.Attributes;

            namespace Consumer;

            {{commandSource}}
            """;
        var compilation = CSharpCompilation.Create(
            "Consumer",
            [CSharpSyntaxTree.ParseText(source)],
            CompilationReferences,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            new IOKode.OpinionatedFramework.SourceGenerators.ResourceControllers.SourceGenerator().AsSourceGenerator());
        driver = driver.RunGenerators(compilation);
        return driver.GetRunResult();
    }

    private static ImmutableArray<MetadataReference> CreateCompilationReferences()
    {
        var references = ((string) AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToDictionary(reference => reference.Display!, reference => (MetadataReference) reference,
                StringComparer.OrdinalIgnoreCase);

        AddReference(references, typeof(Command).Assembly.Location);
        return [.. references.Values];
    }

    private static void AddReference(IDictionary<string, MetadataReference> references, string path) =>
        references[path] = MetadataReference.CreateFromFile(path);
}
