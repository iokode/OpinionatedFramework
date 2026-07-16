using System;
using System.Collections.Generic;
using System.Linq;
using IOKode.OpinionatedFramework.ServiceContainer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Bootstrapping.Abstractions;
using IOKode.OpinionatedFramework.ServiceLocation;

namespace IOKode.OpinionatedFramework.Bootstrapping;

/// <summary>
/// Selects configured drivers, validates them as a group, registers their services, and initializes lifecycle tasks.
/// </summary>
public static class OpinionatedFrameworkBootstrapping
{
    /// <summary>Selects, validates, and registers drivers using the <c>OpinionatedFramework</c> configuration section.</summary>
    /// <param name="configuration">The host configuration used to select and configure drivers.</param>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
    /// <exception cref="BootstrapConfigurationException">Driver selection or validation fails.</exception>
    public static void RegisterDrivers(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var frameworkConfiguration = configuration.GetSection("OpinionatedFramework");
        var sharedState = new Dictionary<string, object>(StringComparer.Ordinal);
        var selectedDrivers = SelectDrivers(configuration, frameworkConfiguration, sharedState);

        var validationErrors = selectedDrivers
            .SelectMany(selectedDriver => selectedDriver.Descriptor.Validate(selectedDriver.Context).Errors)
            .ToArray();
        if (validationErrors.Length > 0)
        {
            throw new BootstrapConfigurationException(validationErrors);
        }

        foreach (var selectedDriver in selectedDrivers)
        {
            selectedDriver.Descriptor.Register(selectedDriver.Context);
        }
    }

    /// <summary>
    /// Initialize the OpinionatedFramework container.
    /// </summary>
    public static void InitializeContainer()
    {
        Container.Initialize();
    }

    /// <summary>Executes lifecycle startup tasks after the container has been initialized.</summary>
    /// <param name="cancellationToken">Cancels lifecycle startup.</param>
    /// <returns>A handle that owns the initialized container.</returns>
    public static async Task<IAsyncDisposable> StartLifecycleAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var startupTasks = Locator.ServiceProvider!.GetServices<IStartupTask>();
            foreach (var startupTask in startupTasks)
            {
                await startupTask.StartAsync(cancellationToken);
            }

            return new ContainerHandle();
        }
        catch
        {
            await Container.Advanced.DisposeAsync();
            throw;
        }
    }
    
    /// <summary>Registers drivers, initializes the service container, and executes startup tasks.</summary>
    /// <param name="configuration">The host configuration used to select and configure drivers.</param>
    /// <param name="cancellationToken">Cancels framework startup tasks.</param>
    /// <returns>A handle that owns the initialized container.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
    /// <exception cref="BootstrapConfigurationException">Driver selection or validation fails.</exception>
    public static async Task<IAsyncDisposable> StartAsync(IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        RegisterDrivers(configuration);
        InitializeContainer();
        return await StartLifecycleAsync(cancellationToken);
    }

    private static IReadOnlyCollection<SelectedDriver> SelectDrivers(IConfiguration configuration,
        IConfigurationSection frameworkConfiguration, IDictionary<string, object> sharedState)
    {
        var selectedDrivers = new List<SelectedDriver>();
        foreach (var contractDrivers in BootstrapDriverCatalog.RegisteredDrivers.GroupBy(driver => driver.ContractType))
        {
            var configurationKeys = contractDrivers
                .Select(driver => driver.ConfigurationKey)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var namedInstanceModes = contractDrivers
                .Select(driver => driver.SupportsNamedInstances)
                .Distinct()
                .ToArray();

            if (configurationKeys.Length != 1 || namedInstanceModes.Length != 1)
            {
                throw new BootstrapConfigurationException(
                    $"Drivers for contract '{contractDrivers.Key.FullName}' must use the same configuration key and instance mode.");
            }

            var configurationSection = frameworkConfiguration.GetSection(configurationKeys[0]);
            if (namedInstanceModes[0])
            {
                foreach (var instanceSection in configurationSection.GetChildren())
                {
                    selectedDrivers.Add(SelectConfiguredDriver(contractDrivers, configuration, frameworkConfiguration,
                        instanceSection, instanceSection.Key, sharedState));
                }

                continue;
            }

            var configuredDriverKey = configurationSection["Driver"];
            if (string.IsNullOrWhiteSpace(configuredDriverKey))
            {
                var defaultDrivers = contractDrivers.Where(driver => driver.IsDefault).ToArray();
                if (defaultDrivers.Length == 0)
                {
                    continue;
                }

                if (defaultDrivers.Length > 1)
                {
                    throw new BootstrapConfigurationException(
                        $"Multiple default drivers are registered for contract '{contractDrivers.Key.FullName}'.");
                }

                selectedDrivers.Add(CreateSelectedDriver(defaultDrivers[0], configuration, frameworkConfiguration,
                    configurationSection, null, sharedState));
                continue;
            }

            selectedDrivers.Add(SelectConfiguredDriver(contractDrivers, configuration, frameworkConfiguration,
                configurationSection, null, sharedState));
        }

        return selectedDrivers;
    }

    private static SelectedDriver SelectConfiguredDriver(IEnumerable<BootstrapDriverDescriptor> availableDrivers,
        IConfiguration configuration, IConfigurationSection frameworkConfiguration,
        IConfigurationSection driverConfiguration, string? instanceName, IDictionary<string, object> sharedState)
    {
        var configuredDriverKey = driverConfiguration["Driver"];
        if (string.IsNullOrWhiteSpace(configuredDriverKey))
        {
            throw new BootstrapConfigurationException(
                $"Configuration '{driverConfiguration.Path}:Driver' is required.");
        }

        var availableDriversArray = availableDrivers.ToArray();
        var selectedDriver = availableDriversArray.SingleOrDefault(driver =>
            string.Equals(driver.DriverKey, configuredDriverKey, StringComparison.OrdinalIgnoreCase));
        if (selectedDriver is null)
        {
            var availableKeys = string.Join(", ", availableDriversArray.Select(driver => driver.DriverKey).Order());
            throw new BootstrapConfigurationException(
                $"Unknown driver '{configuredDriverKey}' for contract '{availableDriversArray[0].ContractType.FullName}'. " +
                $"Available drivers: {availableKeys}. Configuration path: {driverConfiguration.Path}:Driver.");
        }

        return CreateSelectedDriver(selectedDriver, configuration, frameworkConfiguration, driverConfiguration,
            instanceName, sharedState);
    }

    private static SelectedDriver CreateSelectedDriver(BootstrapDriverDescriptor descriptor,
        IConfiguration configuration, IConfigurationSection frameworkConfiguration,
        IConfigurationSection driverConfiguration, string? instanceName, IDictionary<string, object> sharedState)
    {
        return new SelectedDriver(descriptor, new BootstrapDriverContext(
            Container.Services,
            configuration,
            frameworkConfiguration,
            driverConfiguration,
            instanceName,
            sharedState));
    }

    private sealed record SelectedDriver(BootstrapDriverDescriptor Descriptor, BootstrapDriverContext Context);
}
