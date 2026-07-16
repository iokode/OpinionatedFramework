using System.Collections.Generic;

namespace IOKode.OpinionatedFramework.Bootstrapping.Abstractions;

/// <summary>
/// Contains all configuration errors found while validating one bootstrap driver.
/// </summary>
/// <param name="Errors">The configuration errors found by the driver.</param>
public sealed record BootstrapValidationResult(IReadOnlyCollection<BootstrapValidationError> Errors)
{
    /// <summary>
    /// Gets a successful result containing no validation errors.
    /// </summary>
    public static BootstrapValidationResult Success { get; } = new([]);

    /// <summary>
    /// Creates a result containing the supplied validation errors.
    /// </summary>
    /// <param name="errors">The validation errors to include.</param>
    /// <returns>A failed validation result containing <paramref name="errors"/>.</returns>
    public static BootstrapValidationResult Failure(params BootstrapValidationError[] errors) => new(errors);
    
    /// <summary>
    /// Gets whether the driver configuration is valid.
    /// </summary>
    public bool IsValid => Errors.Count == 0;
}

/// <summary>
/// Describes one invalid bootstrap configuration value.
/// </summary>
/// <param name="ConfigurationPath">The complete configuration path that contains the invalid value.</param>
/// <param name="Message">A human-readable description of the validation failure.</param>
public sealed record BootstrapValidationError(
    string ConfigurationPath,
    string Message);
