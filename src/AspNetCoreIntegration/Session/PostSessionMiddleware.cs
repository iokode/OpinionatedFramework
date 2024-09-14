using System.Text.Json;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using Microsoft.AspNetCore.Http;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegration.Session;

public class PostSessionMiddleware : CommandMiddleware
{
    public override async Task ExecuteAsync(ICommandContext context, InvokeNextMiddlewareDelegate nextAsync)
    {
        await nextAsync(context);

        var httpContextAccessor = Locator.Resolve<IHttpContextAccessor>();
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            return;
        }

        foreach ((string key, object? value) in context.PipelineData.ToReadonlyDictionary())
        {
            var serializedValue = JsonSerializer.SerializeToUtf8Bytes(value);
            httpContext.Session.Set($".OF.PipelineData.{key}", serializedValue);
        }
    }
}