using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations;

/// <summary>
/// Binds a command from the body of the request, which holds the properties of the command that are not keys of the
/// resource. The keys come from the route and are not read from the body.
/// </summary>
/// <param name="keyProperties">The names of the properties of the command that are keys of the resource.</param>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class FromCommandBodyAttribute(params string[] keyProperties) : Attribute, IBinderTypeProviderMetadata
{
    /// <summary>
    /// The names of the properties of the command that are keys of the resource.
    /// </summary>
    public IReadOnlyCollection<string> KeyProperties { get; } = keyProperties;

    public BindingSource BindingSource => BindingSource.Body;

    public Type BinderType => typeof(CommandBodyModelBinder);
}
