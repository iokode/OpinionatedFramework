using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.ConfigureApplication;
using IOKode.OpinionatedFramework.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.CommandExecutor;

public partial class CommandExecutorTests
{
    [Fact]
    public async Task InvokeVoidCommand_Success()
    {
        // Arrange
        Container.Clear();
        Container.Services.AddTransient<ICommandExecutor, ContractImplementations.CommandExecutor.CommandExecutor>(_ =>
            new ContractImplementations.CommandExecutor.CommandExecutor(Array.Empty<ICommandMiddleware>()));
        Container.Initialize();

        // Act
        var command = new VoidCommand();
        await command.InvokeAsync();

        // Assert
        Assert.True(command.IsExecuted);
    }

    [Fact]
    public async Task InvokeReturningCommand_Success()
    {
        // Arrange
        Container.Clear();
        Container.Services.AddTransient<ICommandExecutor, ContractImplementations.CommandExecutor.CommandExecutor>(_ =>
            new ContractImplementations.CommandExecutor.CommandExecutor(Array.Empty<ICommandMiddleware>()));
        Container.Initialize();

        // Act
        var command = new ReturningCommand();
        int result = await command.InvokeAsync();

        // Assert
        Assert.Equal(26, result);
    }

    [Fact]
    public async Task InvokeReturningCommandWithParameters_Success()
    {
        // Arrange
        Container.Clear();
        Container.Services.AddTransient<ICommandExecutor, ContractImplementations.CommandExecutor.CommandExecutor>(_ =>
            new ContractImplementations.CommandExecutor.CommandExecutor(Array.Empty<ICommandMiddleware>()));
        Container.Initialize();

        // Act
        var command = new AddTwoNumbersCommand(3, 5);
        int result = await command.InvokeAsync();

        // Assert
        Assert.Equal(8, result);
    }

    [Fact]
    public async Task CommandContext_IsOk()
    {
        // Arrange
        Container.Clear();
        Container.Services.AddTransient<ICommandExecutor, ContractImplementations.CommandExecutor.CommandExecutor>(_ =>
            new ContractImplementations.CommandExecutor.CommandExecutor(Array.Empty<ICommandMiddleware>()));
        Container.Initialize();

        // Act & Assert
        var command = new AssertContextCommand();
        await command.InvokeAsync();
    }

    [Fact]
    public async Task WithSharedData()
    {
        // Arrange
        Container.Clear();
        Container.Services.AddTransient<ICommandExecutor, ContractImplementations.CommandExecutor.CommandExecutor>(_ =>
            new ContractImplementations.CommandExecutor.CommandExecutor(Array.Empty<ICommandMiddleware>(),
                new Dictionary<string, object>
                {
                    { "number1", 2 },
                    { "number2", 3 }
                }));
        Container.Initialize();

        // Act
        var command = new SumNumbersFromSharedDataCommand();
        int result = await command.InvokeAsync();

        // Assert
        Assert.Equal(5, result);
    }

    private class SumNumbersFromSharedDataCommand : Command<int>
    {
        protected override Task<int> ExecuteAsync(CommandContext context)
        {
            int n1 = (int)context.GetSharedData("number1")!;
            int n2 = (int)context.GetSharedData("number2")!;

            return Task.FromResult(n1 + n2);
        }
    }

    private class VoidCommand : Command
    {
        public bool IsExecuted = false;

        protected override Task ExecuteAsync(CommandContext context)
        {
            IsExecuted = true;
            return Task.CompletedTask;
        }
    }

    private class ReturningCommand : Command<int>
    {
        protected override Task<int> ExecuteAsync(CommandContext context)
        {
            return Task.FromResult(26);
        }
    }

    private class AddTwoNumbersCommand : Command<int>
    {
        private readonly int _a;
        private readonly int _b;

        public AddTwoNumbersCommand(int a, int b)
        {
            _a = a;
            _b = b;
        }

        protected override Task<int> ExecuteAsync(CommandContext context)
        {
            return Task.FromResult(_a + _b);
        }
    }

    private class AssertContextCommand : Command
    {
        protected override Task ExecuteAsync(CommandContext context)
        {
            Assert.False(context.IsExecuted);
            Assert.Equal(typeof(AssertContextCommand), context.CommandType);
            Assert.False(context.HasResult);
            Assert.Null(context.Result);

            return Task.CompletedTask;
        }
    }
}