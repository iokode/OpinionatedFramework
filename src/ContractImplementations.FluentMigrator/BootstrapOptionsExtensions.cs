using System;
using IOKode.OpinionatedFramework.Drivers.Abstractions;

namespace IOKode.OpinionatedFramework.ContractImplementations.FluentMigrator;

/// <summary>
/// Contributes the FluentMigrator configuration verb to bootstrap options.
/// </summary>
public static class BootstrapOptionsExtensions
{
    /// <summary>
    /// Configures the FluentMigrator driver with settings that cannot be expressed in configuration.
    /// </summary>
    /// <remarks>
    /// Bootstrap fails when the <c>fluent-migrator</c> driver is not selected, because the configuration would
    /// otherwise be discarded.
    /// </remarks>
    /// <example>
    /// <code>
    /// options.FluentMigrator(migrations =>
    ///     migrations.AddMigrationAssembly(typeof(CreateTaskItems).Assembly));
    /// </code>
    /// </example>
    /// <param name="options">The bootstrap options.</param>
    /// <param name="configure">Configures the driver.</param>
    /// <returns>The same options, to allow chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static IBootstrapOptions FluentMigrator(this IBootstrapOptions options,
        Action<FluentMigratorOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Configure(configure);
        return options;
    }
}
