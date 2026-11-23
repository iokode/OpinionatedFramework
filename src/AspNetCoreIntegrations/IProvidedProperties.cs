namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations;

/// <summary>
/// Tells which properties of an object were given a value, as opposed to keeping the value they were created with.
/// </summary>
public interface IProvidedProperties
{
    /// <summary>
    /// Whether the property named <paramref name="propertyName"/> was given a value.
    /// </summary>
    bool IsProvided(string propertyName);
}
