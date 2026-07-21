using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using IOKode.OpinionatedFramework.Drivers.Abstractions;

namespace IOKode.OpinionatedFramework.ServiceContainer.Drivers;

/// <summary>
/// Describes one generated bootstrap driver registration available to the application.
/// </summary>
public sealed record BootstrapDriverDescriptor(
    Type ContractType,
    string ConfigurationKey,
    string DriverKey,
    bool IsDefault,
    bool SupportsNamedInstances,
    string SourceAssembly,
    Func<BootstrapDriverContext, BootstrapValidationResult> Validate,
    Action<BootstrapDriverContext> Register);

/// <summary>
/// Stores bootstrap driver descriptors available to the application.
/// </summary>
/// <remarks>
/// Generated module initializers populate the catalog before application bootstrap.
/// </remarks>
public static class BootstrapDriverCatalog
{
    private static readonly Lock Sync = new();
    private static readonly List<BootstrapDriverDescriptor> Descriptors = new();

    /// <summary>Gets a snapshot of all drivers available from referenced implementation assemblies.</summary>
    public static IReadOnlyCollection<BootstrapDriverDescriptor> RegisteredDrivers
    {
        get
        {
            lock (Sync)
            {
                return new ReadOnlyCollection<BootstrapDriverDescriptor>(Descriptors.ToArray());
            }
        }
    }

    /// <summary>
    /// Adds a generated driver descriptor.
    /// </summary>
    /// <remarks>
    /// Identical registrations are idempotent. A registration that conflicts with an existing contract and driver key fails.
    /// </remarks>
    /// <typeparam name="TRegistrar">The registrar that validates and registers the driver.</typeparam>
    /// <param name="contractType">The framework contract implemented by the driver.</param>
    /// <param name="configurationKey">The configuration path relative to the framework configuration section.</param>
    /// <param name="driverKey">The configured value that selects the driver.</param>
    /// <param name="isDefault">Whether the driver is selected when no driver is explicitly configured.</param>
    /// <param name="supportsNamedInstances">Whether the configuration section contains multiple named instances.</param>
    /// <param name="sourceAssembly">The assembly that declares the driver.</param>
    /// <exception cref="ArgumentNullException"><paramref name="contractType"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="configurationKey"/> or <paramref name="driverKey"/> is empty.</exception>
    /// <exception cref="BootstrapConfigurationException">A conflicting driver is already registered.</exception>
    public static void Register<TRegistrar>(Type contractType, string configurationKey, string driverKey,
        bool isDefault, bool supportsNamedInstances, string sourceAssembly)
        where TRegistrar : IBootstrapDriverRegistrar
    {
        ArgumentNullException.ThrowIfNull(contractType);
        ArgumentException.ThrowIfNullOrWhiteSpace(configurationKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(driverKey);

        lock (Sync)
        {
            var existingDescriptor = Descriptors.SingleOrDefault(descriptor => descriptor.ContractType == contractType &&
                string.Equals(descriptor.DriverKey, driverKey, StringComparison.OrdinalIgnoreCase));
            if (existingDescriptor is not null &&
                existingDescriptor.ConfigurationKey == configurationKey &&
                existingDescriptor.IsDefault == isDefault &&
                existingDescriptor.SupportsNamedInstances == supportsNamedInstances &&
                existingDescriptor.SourceAssembly == sourceAssembly)
            {
                return;
            }

            if (existingDescriptor is not null)
            {
                throw new BootstrapConfigurationException(
                    $"Driver '{driverKey}' is already registered for contract '{contractType.FullName}'.");
            }

            Descriptors.Add(new BootstrapDriverDescriptor(
                contractType,
                configurationKey,
                driverKey,
                isDefault,
                supportsNamedInstances,
                sourceAssembly,
                TRegistrar.Validate,
                TRegistrar.Register));
        }
    }
}
