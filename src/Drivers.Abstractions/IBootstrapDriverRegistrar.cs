namespace IOKode.OpinionatedFramework.Drivers.Abstractions;

/// <summary>
/// Validates and registers one implementation of a framework contract.
/// </summary>
public interface IBootstrapDriverRegistrar
{
    /// <summary>
    /// Validates the selected driver's configuration without modifying the service collection.
    /// </summary>
    /// <param name="context">The selected driver's bootstrap context.</param>
    /// <returns>A result containing every configuration validation error found by the driver.</returns>
    static abstract BootstrapValidationResult Validate(BootstrapDriverContext context);

    /// <summary>
    /// Registers the selected driver after every selected driver has passed validation.
    /// </summary>
    /// <param name="context">The selected driver's bootstrap context.</param>
    static abstract void Register(BootstrapDriverContext context);
}
