using System;

namespace IOKode.OpinionatedFramework.Drivers.Abstractions;

/// <summary>
/// Collects code-level driver configuration supplied by the application during bootstrap.
/// </summary>
/// <remarks>
/// The framework does not declare capability-specific members on this interface. Each implementation package
/// contributes its own extension methods over it, so a configuration verb is available only when the package
/// that owns it is referenced. Configuration supplied here is applied on top of the values a driver reads from
/// <c>IConfiguration</c>, which remains the baseline.
/// </remarks>
/// <remarks>
/// A verb is named after the driver that owns it rather than after the contract it configures, because several
/// drivers for one contract can be referenced at the same time and contract-named verbs would collide.
/// </remarks>
/// <example>
/// The command executor package contributes <c>CommandExecutor</c>, which cannot be expressed in
/// configuration because a middleware is selected by type.
/// <code>
/// options.CommandExecutor(commands => commands.AddMiddleware&lt;AuditMiddleware&gt;());
/// </code>
/// </example>
public interface IBootstrapOptions
{
    /// <summary>
    /// Adds a delegate that configures the driver options of type <typeparamref name="TOptions"/>.
    /// </summary>
    /// <remarks>
    /// Delegates are applied in the order they were added. Bootstrap fails when no selected driver consumes
    /// <typeparamref name="TOptions"/>, so configuration is never silently discarded.
    /// </remarks>
    /// <typeparam name="TOptions">The options type owned by the driver that consumes the delegate.</typeparam>
    /// <param name="configure">Configures the driver options.</param>
    /// <exception cref="ArgumentNullException"><paramref name="configure"/> is <see langword="null"/>.</exception>
    void Configure<TOptions>(Action<TOptions> configure);
}
