using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Persistence.Queries;

namespace IOKode.OpinionatedFramework.Tests.ResourceControllers.Config;

/// <summary>
/// Sets the tenant parameter of the queries that declare it, which the generated controllers do not bind from the request.
/// </summary>
public class TenantQueryMiddleware : QueryMiddleware
{
    public const string Tenant = "acme";

    public override async Task ExecuteAsync(IQueryExecutionContext executionContext, InvokeNextMiddlewareDelegate nextAsync)
    {
        if (executionContext.HasParameter("tenant"))
        {
            executionContext.SetParameter("tenant", Tenant);
        }

        await nextAsync();
    }
}
