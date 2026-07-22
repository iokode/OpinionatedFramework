using System;
using System.Collections.Generic;
using System.Linq;

namespace IOKode.OpinionatedFramework.Drivers.Abstractions;

/// <summary>
/// Stores the code-level driver configuration supplied by the application and tracks which drivers consumed it.
/// </summary>
public sealed class BootstrapOptions : IBootstrapOptions
{
    private readonly Dictionary<Type, List<Delegate>> configurators = new();
    private readonly HashSet<Type> consumedOptionTypes = new();

    /// <inheritdoc/>
    public void Configure<TOptions>(Action<TOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        if (!this.configurators.TryGetValue(typeof(TOptions), out var optionsConfigurators))
        {
            optionsConfigurators = new List<Delegate>();
            this.configurators.Add(typeof(TOptions), optionsConfigurators);
        }

        optionsConfigurators.Add(configure);
    }

    /// <summary>
    /// Marks <typeparamref name="TOptions"/> as consumed and returns every delegate the application added for it.
    /// </summary>
    /// <remarks>
    /// A driver calls this once per bootstrap even when it expects no configuration, so that unconsumed
    /// application configuration can be reported. Consumption is idempotent, which allows a driver registered for
    /// several contracts or named instances to call it more than once.
    /// </remarks>
    /// <typeparam name="TOptions">The options type owned by the calling driver.</typeparam>
    /// <returns>
    /// A delegate applying every configurator in the order it was added, or <see langword="null"/> when the
    /// application supplied none.
    /// </returns>
    public Action<TOptions>? Consume<TOptions>()
    {
        this.consumedOptionTypes.Add(typeof(TOptions));

        if (!this.configurators.TryGetValue(typeof(TOptions), out var optionsConfigurators))
        {
            return null;
        }

        var configureOptions = optionsConfigurators.Cast<Action<TOptions>>().ToArray();
        return options =>
        {
            foreach (var configure in configureOptions)
            {
                configure(options);
            }
        };
    }

    /// <summary>
    /// Gets the options types the application configured that no selected driver consumed.
    /// </summary>
    public IReadOnlyCollection<Type> UnconsumedOptionTypes =>
        this.configurators.Keys.Where(optionsType => !this.consumedOptionTypes.Contains(optionsType)).ToArray();
}
