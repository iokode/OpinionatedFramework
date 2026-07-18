using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace IOKode.OpinionatedFramework.SourceGenerators.BootstrapDrivers;

/// <summary>
/// Discovers bootstrap driver attributes in referenced assemblies and generates the application driver manifest.
/// </summary>
[Generator]
public sealed class BootstrapDriversGenerator : IIncrementalGenerator
{
    private const string AttributeName = "IOKode.OpinionatedFramework.Drivers.Abstractions.BootstrapDriverAttribute<TContract, TRegistrar>";
    private const string RegistrarName = "IOKode.OpinionatedFramework.Drivers.Abstractions.IBootstrapDriverRegistrar";

    private static readonly DiagnosticDescriptor DuplicateDriver = new(
        "OF0002",
        "Duplicate bootstrap driver",
        "Driver '{0}' for contract '{1}' is declared by both '{2}' and '{3}'",
        "Bootstrapping",
        DiagnosticSeverity.Error,
        true);

    private static readonly DiagnosticDescriptor InvalidRegistrar = new(
        "OF0003",
        "Invalid bootstrap driver registrar",
        "Registrar '{0}' declared by '{1}' does not implement IBootstrapDriverRegistrar",
        "Bootstrapping",
        DiagnosticSeverity.Error,
        true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var driverAssemblies = context.MetadataReferencesProvider
            .Combine(context.CompilationProvider)
            .Select((pair, _) => pair.Right.GetAssemblyOrModuleSymbol(pair.Left) as IAssemblySymbol)
            .Where(assembly => assembly is not null)
            .Collect();

        var drivers = driverAssemblies
            .Combine(context.CompilationProvider)
            .Select((pair, _) => GetDrivers(pair.Left!, pair.Right));

        context.RegisterSourceOutput(drivers, Generate);
    }

    private static ImmutableArray<DriverInfo> GetDrivers(ImmutableArray<IAssemblySymbol?> assemblies,
        Compilation compilation)
    {
        var registrarInterface = compilation.GetTypeByMetadataName(RegistrarName);
        var drivers = ImmutableArray.CreateBuilder<DriverInfo>();

        foreach (var assembly in assemblies.Where(assembly => assembly is not null).Cast<IAssemblySymbol>())
        {
            foreach (var attribute in assembly.GetAttributes().Where(attribute =>
                         attribute.AttributeClass?.OriginalDefinition.ToDisplayString() == AttributeName))
            {
                if (attribute.AttributeClass is not { TypeArguments: [INamedTypeSymbol contractType, _] } attributeClass ||
                    attributeClass.TypeArguments[1] is not INamedTypeSymbol registrarType ||
                    attribute.ConstructorArguments.Length != 4 ||
                    attribute.ConstructorArguments[0].Value is not string configurationKey ||
                    attribute.ConstructorArguments[1].Value is not string driverKey ||
                    attribute.ConstructorArguments[2].Value is not bool isDefault ||
                    attribute.ConstructorArguments[3].Value is not bool supportsNamedInstances)
                {
                    continue;
                }

                var registrarIsValid = registrarInterface is not null && registrarType.AllInterfaces.Any(@interface =>
                    SymbolEqualityComparer.Default.Equals(@interface, registrarInterface));

                drivers.Add(new DriverInfo(
                    contractType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    contractType.ToDisplayString(),
                    configurationKey,
                    driverKey,
                    registrarType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    registrarType.ToDisplayString(),
                    isDefault,
                    supportsNamedInstances,
                    assembly.Name,
                    registrarIsValid));
            }
        }

        return drivers.ToImmutable();
    }

    private static void Generate(SourceProductionContext context, ImmutableArray<DriverInfo> drivers)
    {
        foreach (var invalidDriver in drivers.Where(driver => !driver.RegistrarIsValid))
        {
            context.ReportDiagnostic(Diagnostic.Create(InvalidRegistrar, Location.None,
                invalidDriver.RegistrarDisplayName, invalidDriver.SourceAssembly));
        }

        var validDrivers = drivers.Where(driver => driver.RegistrarIsValid).ToArray();
        var duplicateGroups = validDrivers.GroupBy(driver =>
            $"{driver.ContractType}|{driver.DriverKey}", StringComparer.OrdinalIgnoreCase);
        var duplicateKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var duplicateGroup in duplicateGroups.Where(group => group.Count() > 1))
        {
            var duplicateDrivers = duplicateGroup.Take(2).ToArray();
            duplicateKeys.Add(duplicateGroup.Key);
            context.ReportDiagnostic(Diagnostic.Create(DuplicateDriver, Location.None,
                duplicateDrivers[0].DriverKey,
                duplicateDrivers[0].ContractDisplayName,
                duplicateDrivers[0].SourceAssembly,
                duplicateDrivers[1].SourceAssembly));
        }

        var source = new StringBuilder();
        source.AppendLine("// <auto-generated />");
        source.AppendLine("#nullable enable");
        source.AppendLine("namespace IOKode.OpinionatedFramework.Bootstrapping.Generated;");
        source.AppendLine("internal static class BootstrapDriverManifest");
        source.AppendLine("{");
        source.AppendLine("    [global::System.Runtime.CompilerServices.ModuleInitializer]");
        source.AppendLine("    internal static void Register()");
        source.AppendLine("    {");

        foreach (var driver in validDrivers.Where(driver =>
                     !duplicateKeys.Contains($"{driver.ContractType}|{driver.DriverKey}")))
        {
            source.AppendLine($"        global::IOKode.OpinionatedFramework.ServiceContainer.Drivers.BootstrapDriverCatalog.Register<{driver.RegistrarType}>(");
            source.AppendLine($"            typeof({driver.ContractType}),");
            source.AppendLine($"            {Literal(driver.ConfigurationKey)},");
            source.AppendLine($"            {Literal(driver.DriverKey)},");
            source.AppendLine($"            {driver.IsDefault.ToString().ToLowerInvariant()},");
            source.AppendLine($"            {driver.SupportsNamedInstances.ToString().ToLowerInvariant()},");
            source.AppendLine($"            {Literal(driver.SourceAssembly)});");
        }

        source.AppendLine("    }");
        source.AppendLine("}");
        context.AddSource("BootstrapDriverManifest.g.cs", SourceText.From(source.ToString(), Encoding.UTF8));
    }

    private static string Literal(string value) => SymbolDisplay.FormatLiteral(value, true);

    private sealed record DriverInfo(
        string ContractType,
        string ContractDisplayName,
        string ConfigurationKey,
        string DriverKey,
        string RegistrarType,
        string RegistrarDisplayName,
        bool IsDefault,
        bool SupportsNamedInstances,
        string SourceAssembly,
        bool RegistrarIsValid);
}
