using System.Threading.Tasks;
using IOKode.OpinionatedFramework.AspNetCoreIntegrations.Exceptions;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Exceptions;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations.Middlewares;

public class ResourceNotFoundCommandMiddleware : CommandMiddleware
{
    public override async Task ExecuteAsync(ICommandExecutionContext executionContext, InvokeNextMiddlewareDelegate nextAsync)
    {
        try
        {
            await nextAsync();
        }
        catch (EntityNotFoundException)
        {
            throw new ResourceNotFoundException();
        }
        catch (EmptyResultException)
        {
            throw new ResourceNotFoundException();
        }
    }
}