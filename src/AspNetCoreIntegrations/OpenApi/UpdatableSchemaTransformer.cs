using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations.OpenApi;

/// <summary>
/// Describes an <see cref="Updatable{T}"/> as the value it holds, which is how it is read.
/// </summary>
public sealed class UpdatableSchemaTransformer : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;
        if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Updatable<>))
        {
            return;
        }

        var valueSchema = await context.GetOrCreateSchemaAsync(type.GetGenericArguments()[0], null, cancellationToken);
        schema.Type = null;
        schema.Properties = null;
        schema.Required = null;
        schema.AllOf = new List<IOpenApiSchema> { valueSchema };
    }
}
