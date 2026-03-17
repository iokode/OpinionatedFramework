using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Resources.Exceptions;
using Microsoft.AspNetCore.Http;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations.Middlewares;

public class ResourceNotFoundControllerMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ResourceNotFoundException)
        {
            if (!context.Response.HasStarted)
            {
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            }
        }
    }
}