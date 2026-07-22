using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using IOKode.OpinionatedFramework.Drivers.Abstractions;
using IOKode.OpinionatedFramework.ServiceContainer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.BootstrapDriversGenerator;

public class BootstrapDriversGeneratorTests
{
    private static readonly ImmutableArray<MetadataReference> CompilationReferences = CreateCompilationReferences();

    [Fact]
    public void DriverFromPackageMetadataReferenceIsDiscovered()
    {
        var packageReference = CompileDriverAssembly("ThirdParty.Driver", "third-party");
        var runResult = RunGenerator([packageReference]);

        Assert.Empty(runResult.Diagnostics);
        var generatedSource = Assert.Single(runResult.Results).GeneratedSources.Single().SourceText.ToString();
        Assert.Contains("global::ThirdParty.DriverRegistrar", generatedSource);
        Assert.Contains("\"third-party\"", generatedSource);
    }

    [Fact]
    public void DuplicateContractAndDriverKeyProducesDiagnostic()
    {
        var firstPackage = CompileDriverAssembly("First.Driver", "duplicate");
        var secondPackage = CompileDriverAssembly("Second.Driver", "duplicate");
        var runResult = RunGenerator([firstPackage, secondPackage]);

        var diagnostic = Assert.Single(runResult.Diagnostics, diagnostic => diagnostic.Id == "OF0002");
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [Fact]
    public void ReservedDriverKeyProducesDiagnostic()
    {
        var packageReference = CompileDriverAssembly("Reserved.Driver", "none");
        var runResult = RunGenerator([packageReference]);

        var diagnostic = Assert.Single(runResult.Diagnostics, diagnostic => diagnostic.Id == "OF0004");
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);

        var generatedSource = Assert.Single(runResult.Results).GeneratedSources.Single().SourceText.ToString();
        Assert.DoesNotContain("\"none\"", generatedSource);
    }

    private static GeneratorDriverRunResult RunGenerator(IEnumerable<MetadataReference> packageReferences)
    {
        var compilation = CSharpCompilation.Create(
            "Consumer",
            [CSharpSyntaxTree.ParseText("public static class Program { }")],
            CompilationReferences.AddRange(packageReferences),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            new IOKode.OpinionatedFramework.SourceGenerators.BootstrapDrivers.BootstrapDriversGenerator()
                .AsSourceGenerator());
        driver = driver.RunGenerators(compilation);
        return driver.GetRunResult();
    }

    private static MetadataReference CompileDriverAssembly(string assemblyName, string driverKey)
    {
        var source = $$"""
            using IOKode.OpinionatedFramework.Drivers.Abstractions;

            [assembly: BootstrapDriver<
                ThirdParty.IStartupContract,
                ThirdParty.DriverRegistrar>("Startup", "{{driverKey}}")]

            namespace ThirdParty;

            public interface IStartupContract;

            public sealed class DriverRegistrar : IBootstrapDriverRegistrar
            {
                public static BootstrapValidationResult Validate(BootstrapDriverContext context)
                {
                    return BootstrapValidationResult.Success;
                }

                public static void Register(BootstrapDriverContext context)
                {
                }
            }
            """;
        var compilation = CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText(source)],
            CompilationReferences,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var assemblyStream = new MemoryStream();
        var emitResult = compilation.Emit(assemblyStream);
        Assert.True(emitResult.Success, string.Join(Environment.NewLine, emitResult.Diagnostics));
        return MetadataReference.CreateFromImage(assemblyStream.ToArray());
    }

    private static ImmutableArray<MetadataReference> CreateCompilationReferences()
    {
        var references = ((string) AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToDictionary(reference => reference.Display!, reference => (MetadataReference) reference,
                StringComparer.OrdinalIgnoreCase);

        AddReference(references, typeof(BootstrapDriverAttribute<,>).Assembly.Location);
        AddReference(references, typeof(IOpinionatedServiceCollection).Assembly.Location);
        AddReference(references, typeof(IConfiguration).Assembly.Location);
        return [.. references.Values];
    }

    private static void AddReference(IDictionary<string, MetadataReference> references, string path) =>
        references[path] = MetadataReference.CreateFromFile(path);
}
