using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.ConfigureApplication;
using IOKode.OpinionatedFramework.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.CommandExecutor;

public partial class CommandExecutorTests
{
    private static readonly List<string> _middlewares = new();

    [Fact]
    public async Task MiddlewareIsInvoked_Success()
    {
        // Arrange
        TestMiddleware.Counter = 0;
        Container.Clear();
        Container.Services.AddTransient<ICommandExecutor, ContractImplementations.CommandExecutor.CommandExecutor>(_ => new ContractImplementations.CommandExecutor.CommandExecutor(new ICommandMiddleware[] { new TestMiddleware() }));
        Container.Initialize();

        // Act
        var command = new VoidCommand();
        await command.InvokeAsync();

        // Assert
        Assert.Equal(1, TestMiddleware.Counter);
    }

    [Fact]
    public async Task MiddlewarePipelineExecutesInOrder_Success()
    {
        // Arrange
        _middlewares.Clear();
        TestMiddleware.Counter = 0;
        TestMiddleware2.Counter = 0;
        Container.Clear();
        Container.Services.AddTransient<ICommandExecutor, ContractImplementations.CommandExecutor.CommandExecutor>(_ =>
            new ContractImplementations.CommandExecutor.CommandExecutor(new ICommandMiddleware[] { new TestMiddleware(), new TestMiddleware2() }));
        Container.Initialize();

        // Act
        var command = new VoidCommand();
        await command.InvokeAsync();

        // Assert
        Assert.Equal(1, TestMiddleware.Counter);
        Assert.Equal(1, TestMiddleware2.Counter);
        Assert.Equal(2, _middlewares.Count);
        Assert.Equal(new List<string> {"Middleware", "Middleware2"}, _middlewares);
    }

    [Fact]
    public async Task MiddlewareHandlesExceptions_Success()
    {
        // Arrange
        Container.Clear();
        Container.Services.AddTransient<ICommandExecutor, ContractImplementations.CommandExecutor.CommandExecutor>(_ =>
            new ContractImplementations.CommandExecutor.CommandExecutor(new ICommandMiddleware[] { new ExceptionHandlingMiddleware() }));
        Container.Initialize();

        // Act
        var command = new ExceptionThrowingCommand();
        await command.InvokeAsync();

        // Assert
        Assert.True(ExceptionHandlingMiddleware.ExceptionHandled);
    }

    [Fact]
    public async Task CommandContextIsOk_Success()
    {
        // Arrange
        Container.Clear();
        Container.Services.AddTransient<ICommandExecutor, ContractImplementations.CommandExecutor.CommandExecutor>(_ =>
            new ContractImplementations.CommandExecutor.CommandExecutor(new ICommandMiddleware[] { new AssertContextMiddlewareBeforeCommand(), new AssertContextMiddlewareAfterCommand() }));
        Container.Initialize();
        
        // Act & Assert (assertions are inside middlewares)
        var command = new AddTwoNumbersCommand(5, 3);
        await command.InvokeAsync();
    }

    private class TestMiddleware : ICommandMiddleware
    {
        public static int Counter { get; set; } = 0;

        public async Task ExecuteAsync(CommandContext context, InvokeNextMiddlewareDelegate next)
        {
            _middlewares.Add("Middleware");
            Counter++;
            await next(context);
        }
    }

    private class TestMiddleware2 : ICommandMiddleware
    {
        public static int Counter { get; set; } = 0;

        public async Task ExecuteAsync(CommandContext context, InvokeNextMiddlewareDelegate next)
        {
            _middlewares.Add("Middleware2");
            Counter++;
            await next(context);
        }
    }

    private class ExceptionHandlingMiddleware : ICommandMiddleware
    {
        public static bool ExceptionHandled { get; set; } = false;

        public async Task ExecuteAsync(CommandContext context, InvokeNextMiddlewareDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception)
            {
                ExceptionHandled = true;
            }
        }
    }

    private class ExceptionThrowingCommand : Command
    {
        protected override Task ExecuteAsync(CommandContext context)
        {
            throw new Exception("Test exception");
        }
    }
    
    private class AssertContextMiddlewareBeforeCommand : ICommandMiddleware
    {
        public async Task ExecuteAsync(CommandContext context, InvokeNextMiddlewareDelegate next)
        {
            Assert.Equal(typeof(AddTwoNumbersCommand), context.CommandType);
            Assert.False(context.HasResult);
            Assert.Null(context.Result);
            Assert.False(context.IsExecuted);

            await next(context);
        }
    }

    private class AssertContextMiddlewareAfterCommand : ICommandMiddleware
    {
        public async Task ExecuteAsync(CommandContext context, InvokeNextMiddlewareDelegate next)
        {
            await next(context);
            
            Assert.Equal(typeof(AddTwoNumbersCommand), context.CommandType);
            Assert.True(context.HasResult);
            Assert.Equal(8, context.Result);
            Assert.True(context.IsExecuted);
        }
    }
}