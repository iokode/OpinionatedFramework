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
        var executor = Helpers.CreateExecutor();
        
        // Act & Assert
        var cmd = new VoidCommand();
        await executor.InvokeAsync(cmd, default);
    }

    private abstract class PreparedCommandBase : Command
    {
        protected bool IsPrepared;
        protected CommandContext Ctx = null!;

        protected override Task PrepareAsync(CommandContext context)
        {
            IsPrepared = true;
            Ctx = context;
            return Task.CompletedTask;
        }
    }

    private class VoidCommand : PreparedCommandBase
    {
        protected override Task ExecuteAsync(CommandContext context)
        {
            Assert.True(IsPrepared);
            Assert.Same(context, Ctx);
            
            return Task.CompletedTask;
        }
    }
}