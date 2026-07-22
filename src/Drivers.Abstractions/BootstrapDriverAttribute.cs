using System;

namespace IOKode.OpinionatedFramework.Drivers.Abstractions;

/// <summary>
/// Declares that an assembly provides a named bootstrap driver for <typeparamref name="TContract"/>.
/// </summary>
/// <remarks>
/// The bootstrap driver source generator discovers this attribute from referenced assemblies.
/// </remarks>
/// <typeparam name="TContract">The framework contract implemented by the driver.</typeparam>
/// <typeparam name="TRegistrar">The registrar that validates configuration and registers the implementation.</typeparam>
/// <param name="configurationKey">The configuration path relative to <c>OpinionatedFramework</c>.</param>
/// <param name="driverKey">The value used by the section's <c>Driver</c> property to select this implementation.</param>
/// <param name="isDefault">Whether this driver is selected when the contract has no explicitly configured driver.</param>
/// <param name="supportsNamedInstances">Whether the configuration section contains multiple named instances.</param>
/// <example>
/// A storage contract can use named instances to configure multiple independently named disks.
/// </example>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class BootstrapDriverAttribute<TContract, TRegistrar>(
    string configurationKey,
    string driverKey,
    bool isDefault = false,
    bool supportsNamedInstances = false) : Attribute
    where TRegistrar : IBootstrapDriverRegistrar
{
    /// <summary>Gets the configuration path relative to <c>OpinionatedFramework</c>.</summary>
    public string ConfigurationKey { get; } = configurationKey;
    /// <summary>Gets the configured value that selects this driver.</summary>
    public string DriverKey { get; } = driverKey;
    /// <summary>Gets whether this driver is selected when no driver is explicitly configured.</summary>
    public bool IsDefault { get; } = isDefault;
    /// <summary>Gets whether this driver is created once for each named child configuration section.</summary>
    public bool SupportsNamedInstances { get; } = supportsNamedInstances;
}
