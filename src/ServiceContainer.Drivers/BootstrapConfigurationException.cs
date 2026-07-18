using System;
using System.Collections.Generic;
using System.Linq;
using IOKode.OpinionatedFramework.Drivers.Abstractions;

namespace IOKode.OpinionatedFramework.ServiceContainer.Drivers;

/// <summary>
/// Represents invalid configuration discovered while selecting or validating bootstrap drivers.
/// </summary>
public class BootstrapConfigurationException : Exception
{
    /// <summary>
    /// Creates an exception for a driver-selection configuration error.
    /// </summary>
    public BootstrapConfigurationException(string message) : base(message)
    {
        Errors = [];
    }

    /// <summary>
    /// Creates an exception that aggregates all driver validation errors.
    /// </summary>
    public BootstrapConfigurationException(IReadOnlyCollection<BootstrapValidationError> errors)
        : base(CreateMessage(errors))
    {
        Errors = errors;
    }

    /// <summary>
    /// Gets the structured driver validation errors associated with this exception.
    /// </summary>
    public IReadOnlyCollection<BootstrapValidationError> Errors { get; }

    private static string CreateMessage(IReadOnlyCollection<BootstrapValidationError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        return "Bootstrap configuration validation failed:" + Environment.NewLine +
               string.Join(Environment.NewLine,
                   errors.Select(error => $"- {error.ConfigurationPath}: {error.Message}"));
    }
}
