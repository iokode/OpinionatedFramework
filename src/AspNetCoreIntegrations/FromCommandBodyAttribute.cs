using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations;

/// <summary>
/// Binds a command from the body of the request, which holds the properties of the command that are not keys of the
/// resource. The keys come from the route and are not read from the body.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class FromCommandBodyAttribute : Attribute, IBinderTypeProviderMetadata
{
    /// <param name="keyProperties">The names of the properties of the command that are keys of the resource.</param>
    /// <exception cref="ArgumentNullException"><paramref name="keyProperties"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// One of <paramref name="keyProperties"/> is <see langword="null"/>, empty or white space.
    /// </exception>
    public FromCommandBodyAttribute(params string[] keyProperties)
    {
        ArgumentNullException.ThrowIfNull(keyProperties);
        for (var index = 0; index < keyProperties.Length; index++)
        {
            if (string.IsNullOrWhiteSpace(keyProperties[index]))
            {
                throw new ArgumentException(
                    $"The key property at index {index} is null, empty or white space; a key property is the name of a property of the command.",
                    nameof(keyProperties));
            }
        }

        KeyProperties = keyProperties;
    }

    /// <summary>
    /// The names of the properties of the command that are keys of the resource.
    /// </summary>
    public IReadOnlyCollection<string> KeyProperties { get; }

    public BindingSource BindingSource => BindingSource.Body;

    public Type BinderType => typeof(CommandBodyModelBinder);
}
