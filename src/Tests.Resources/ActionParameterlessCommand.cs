using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Resources.Attributes;

namespace IOKode.OpinionatedFramework.Tests.Resources;

[ActionOnResource("orders", "do action")]
public class ActionParameterlessCommand : Command
{
    protected override Task ExecuteAsync(ICommandExecutionContext executionContext)
    {
        return Task.CompletedTask;
    }
}