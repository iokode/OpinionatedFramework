using IOKode.OpinionatedFramework.Commands;

namespace Tests.AspNetCoreIntegration.SessionEntryPoint;

public class Command1 : Command
{
    protected override Task ExecuteAsync(ICommandContext context)
    {
        context.PipelineData.Set("Command1", "Command1 Value");
        return Task.CompletedTask;
    }
}