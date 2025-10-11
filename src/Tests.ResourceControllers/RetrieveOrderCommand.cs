using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Resources.Attributes;

namespace IOKode.OpinionatedFramework.Tests.ResourceControllers;

/// <summary>
/// This command is in the same project as the test to ensure that we can generate controllers
/// from commands that exists in the same project.
/// </summary>
[RetrieveResource("order")]
public class RetrieveOrderCommand(int id) : Command<string>
{
    protected override Task<string> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        return Task.FromResult($"order-{id}");
    }
}