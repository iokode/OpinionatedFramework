using System.Text.Json;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using Microsoft.AspNetCore.Http;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegration.Session;

public class PreSessionMiddleware : CommandMiddleware
{
    public override async Task ExecuteAsync(ICommandContext context, InvokeNextMiddlewareDelegate nextAsync)
    {
        var httpContextAccessor = Locator.Resolve<IHttpContextAccessor>();
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            await nextAsync(context);
            return;
        }

        foreach (var key in httpContext.Session.Keys)
        {
            var value = httpContext.Session.Get(key);
            if (value != null)
            {
                if (key.StartsWith(".OF.PipelineData."))
                {
                    string trimmedKey = key[".OF.PipelineData.".Length..];
                    var deserializedValue = JsonSerializer.Deserialize<object>(value);
                    context.PipelineData.Set(trimmedKey, deserializedValue);
                }
            }
        }
        await nextAsync(context);
    }
}