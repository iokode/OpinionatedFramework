using System;
using IOKode.OpinionatedFramework.Drivers.Abstractions;
using Microsoft.Extensions.Logging;

namespace IOKode.OpinionatedFramework.ContractImplementations.MicrosoftLogging;

/// <summary>
/// Contributes the Microsoft logging configuration verb to bootstrap options.
/// </summary>
public static class BootstrapOptionsExtensions
{
    /// <summary>
    /// Configures the Microsoft logging builder with settings that cannot be expressed in configuration.
    /// </summary>
    /// <remarks>
    /// The delegate runs after the driver applies the configured minimum level and the console provider, so it can
    /// add providers and filters, or call <see cref="LoggingBuilderExtensions.ClearProviders"/> to replace the
    /// console entirely. Bootstrap fails when the Microsoft logging driver is not selected, because the
    /// configuration would otherwise be discarded.
    /// </remarks>
    /// <example>
    /// <code>
    /// options.MicrosoftLogging(logging => logging.AddFilter("Microsoft", LogLevel.Warning));
    /// </code>
    /// </example>
    /// <param name="options">The bootstrap options.</param>
    /// <param name="configure">Configures the logging builder.</param>
    /// <returns>The same options, to allow chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static IBootstrapOptions MicrosoftLogging(this IBootstrapOptions options,
        Action<ILoggingBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Configure(configure);
        return options;
    }
}
