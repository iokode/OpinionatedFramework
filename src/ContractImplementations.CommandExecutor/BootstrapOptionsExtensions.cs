using System;
using IOKode.OpinionatedFramework.Drivers.Abstractions;

namespace IOKode.OpinionatedFramework.ContractImplementations.CommandExecutor;

/// <summary>
/// Contributes the command executor configuration verb to bootstrap options.
/// </summary>
public static class BootstrapOptionsExtensions
{
    /// <summary>
    /// Configures the default command executor with settings that cannot be expressed in configuration.
    /// </summary>
    /// <remarks>
    /// Referencing this package makes the verb available, which is not the same as its driver being selected.
    /// Bootstrap fails when the <c>default</c> driver is not the selected one, because the configuration would
    /// otherwise be discarded. That happens when the contract is configured with <c>"Driver": "none"</c> so the
    /// application can register a command executor itself, or when another command executor driver is
    /// configured.
    /// </remarks>
    /// <example>
    /// A middleware is selected by type, so it can only be added from code.
    /// <code>
    /// options.CommandExecutor(commands => commands.AddMiddleware&lt;AuditMiddleware&gt;());
    /// </code>
    /// </example>
    /// <param name="options">The bootstrap options.</param>
    /// <param name="configure">Configures the command executor.</param>
    /// <returns>The same options, to allow chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static IBootstrapOptions CommandExecutor(this IBootstrapOptions options,
        Action<CommandExecutorOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Configure(configure);
        return options;
    }
}
