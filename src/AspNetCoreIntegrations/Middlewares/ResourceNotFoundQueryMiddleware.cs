using System.Threading.Tasks;
using IOKode.OpinionatedFramework.AspNetCoreIntegrations.Exceptions;
using IOKode.OpinionatedFramework.Persistence.Queries;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Exceptions;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations.Middlewares;

public class ResourceNotFoundQueryMiddleware : QueryMiddleware
{
    public override async Task ExecuteAsync(IQueryExecutionContext executionContext, InvokeNextMiddlewareDelegate nextAsync)
    {
        try
        {
            await nextAsync();
        }
        catch (EmptyResultException)
        {
            throw new ResourceNotFoundException();
        }
    }
}