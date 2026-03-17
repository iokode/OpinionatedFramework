using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations.Middlewares;

public class UnhandledExceptionControllerMiddleware(IProblemDetailsService problemDetailsService) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var problem = new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred. Please contact support if the problem persists.",
                Status = StatusCodes.Status500InternalServerError
            };

            await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = problem,
                Exception = ex
            });
        }
    }
}