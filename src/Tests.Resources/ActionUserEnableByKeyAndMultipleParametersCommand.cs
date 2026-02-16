using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Resources.Attributes;

namespace IOKode.OpinionatedFramework.Tests.Resources;

[ActionOnResource("user", key: "code", action: "enable")]
public class ActionUserEnableByKeyAndMultipleParametersCommand(string code, string enableName, bool enable) : Command<string>
{
    protected override Task<string> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        return Task.FromResult($"enabled-by-code-{code}-{enableName}-{enable}");
    }
}