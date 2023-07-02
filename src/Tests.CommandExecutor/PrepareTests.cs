using System;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.ConfigureApplication;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.CommandExecutor;

public class PrepareTests
{
    [Fact]
    public async Task PreparedCommand_IsPrepared()
    {
        // Arrange
        Container.Clear();
        Container.Services.AddTransient<ICommandExecutor, ContractImplementations.CommandExecutor.CommandExecutor>(_ =>
            new ContractImplementations.CommandExecutor.CommandExecutor(Array.Empty<ICommandMiddleware>()));
        Container.Initialize();
        
        // Act & Assert
        var cmd = new VoidCommand();
        await cmd.InvokeAsync();
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