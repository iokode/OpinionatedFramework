using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Persistence.Queries;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Exceptions;
using IOKode.OpinionatedFramework.Resources.Exceptions;

namespace IOKode.OpinionatedFramework.Resources.Middlewares;

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