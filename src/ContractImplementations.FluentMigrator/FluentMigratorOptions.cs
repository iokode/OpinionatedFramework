using System;
using System.Collections.Generic;
using System.Reflection;

namespace IOKode.OpinionatedFramework.ContractImplementations.FluentMigrator;

/// <summary>
/// Collects the code-level configuration of the <c>fluent-migrator</c> driver.
/// </summary>
public sealed class FluentMigratorOptions
{
    private readonly List<Assembly> migrationAssemblies = new();

    /// <summary>
    /// Adds an assembly to scan for migrations.
    /// </summary>
    /// <remarks>
    /// An assembly is compile-time identity, so it is supplied here rather than through configuration, where a
    /// misspelled name would only fail once the application starts.
    /// </remarks>
    /// <example>
    /// <code>
    /// migrations.AddMigrationAssembly(typeof(CreateTaskItems).Assembly);
    /// </code>
    /// </example>
    /// <param name="assembly">The assembly containing migrations.</param>
    /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
    public void AddMigrationAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        this.migrationAssemblies.Add(assembly);
    }

    internal IReadOnlyList<Assembly> MigrationAssemblies => this.migrationAssemblies;
}
