using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations.OpenApi;

/// <summary>
/// Describes the body of a command bound with <see cref="FromCommandBodyAttribute"/> with the properties of the
/// command that are not keys of the resource, which come from the route.
/// </summary>
public sealed class CommandBodySchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.ParameterDescription?.ParameterDescriptor is not ControllerParameterDescriptor parameter
            || parameter.ParameterType != context.JsonTypeInfo.Type
            || parameter.ParameterInfo.GetCustomAttribute<FromCommandBodyAttribute>() is not { } attribute)
        {
            return Task.CompletedTask;
        }

        var keyNames = context.JsonTypeInfo.Properties
            .Where(property => property.AttributeProvider is PropertyInfo clrProperty && attribute.KeyProperties.Contains(clrProperty.Name))
            .Select(property => property.Name);
        foreach (var keyName in keyNames)
        {
            schema.Properties?.Remove(keyName);
            schema.Required?.Remove(keyName);
        }

        return Task.CompletedTask;
    }
}
