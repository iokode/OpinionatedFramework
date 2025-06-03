using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;

namespace IOKode.OpinionatedFramework.Tests.AspNetCoreCommandControllers;

public class TestCommand : Command
{
    protected override Task ExecuteAsync(ICommandExecutionContext executionContext)
    {
        return Task.CompletedTask;
    }
}

public abstract class TestCommand<TResult> : Command<TResult>
{
    protected override Task<TResult> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        return Task.FromResult(default(TResult)!);
    }
}

public class Test2Command(int id) : TestCommand<int>
{
    
    protected override Task<int> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        return Task.FromResult(id);
    }
}