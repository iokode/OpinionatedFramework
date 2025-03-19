using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.CommandExecutor;

public class PrepareTests
{
    [Fact]
    public async Task PreparedCommand_IsPrepared()
    {
        // Arrange
        var executor = Helpers.CreateExecutor(_ => { });
        
        // Act & Assert
        var cmd = new VoidCommand();
        await executor.InvokeAsync(cmd, CancellationToken.None);
    }

    private abstract class PreparedCommandBase : Command
    {
        protected bool IsPrepared;
        protected ICommandExecutionContext Ctx = null!;

        protected override Task PrepareAsync(ICommandExecutionContext executionContext)
        {
            IsPrepared = true;
            Ctx = executionContext;
            return Task.CompletedTask;
        }
    }

    private class VoidCommand : PreparedCommandBase
    {
        protected override Task ExecuteAsync(ICommandExecutionContext executionContext)
        {
            Assert.True(IsPrepared);
            Assert.Same(executionContext, Ctx);
            
            return Task.CompletedTask;
        }
    }
}