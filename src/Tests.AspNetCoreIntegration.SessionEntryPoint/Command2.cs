using IOKode.OpinionatedFramework.Commands;

namespace Tests.AspNetCoreIntegration.SessionEntryPoint;

public class Command2 : Command<string>
{
    protected override Task<string> ExecuteAsync(ICommandContext context)
    {
        var pipelineData = context.PipelineData.Get<string>("Command1");

        return Task.FromResult(pipelineData);
    }
}