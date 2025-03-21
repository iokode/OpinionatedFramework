using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.CommandExecutor;

public class MiddlewareTests
{
    public static readonly List<string> ExecutedMiddlewares = new();

    [Fact]
    public async Task MiddlewareIsInvoked_Success()
    {
        // Arrange
        CounterMiddleware.Counter = 0;
        var executor = Helpers.CreateExecutor(options => options.AddMiddleware<CounterMiddleware>());

        // Act
        var cmd = new VoidCommand();
        await executor.InvokeAsync(cmd, CancellationToken.None);

        // Assert
        Assert.Equal(1, CounterMiddleware.Counter);
    }

    [Fact]
    public async Task MiddlewarePipelineExecutesInOrder_Success()
    {
        // Arrange
        ExecutedMiddlewares.Clear();
        CounterMiddleware.Counter = 0;
        CounterMiddleware2.Counter = 0;
        var executor = Helpers.CreateExecutor(options =>
        {
            options.AddMiddleware<CounterMiddleware>();
            options.AddMiddleware<CounterMiddleware2>();
        });

        // Act
        var cmd = new VoidCommand();
        await executor.InvokeAsync(cmd, CancellationToken.None);

        // Assert
        Assert.Equal(1, CounterMiddleware.Counter);
        Assert.Equal(1, CounterMiddleware2.Counter);
        Assert.Equal(2, ExecutedMiddlewares.Count);
        Assert.Equal(new List<string> { "Counter", "Counter2" }, ExecutedMiddlewares);
    }

    [Fact]
    public async Task MiddlewareHandlesExceptions_Success()
    {
        // Arrange
        var executor = Helpers.CreateExecutor(options => options.AddMiddleware<ExceptionHandlingMiddleware>());

        // Act
        var cmd = new ExceptionThrowingCommand();
        await executor.InvokeAsync(cmd, CancellationToken.None);

        // Assert
        Assert.True(ExceptionHandlingMiddleware.ExceptionHandled);
    }

    [Fact]
    public async Task CommandContextInMiddleware_PropertiesAreOk()
    {
        // Arrange
        var executor = Helpers.CreateExecutor(options =>
        {
            options.AddMiddleware<AssertContextMiddlewareBeforeCommand>();
            options.AddMiddleware<AssertContextMiddlewareAfterCommand>();
        });

        // Act & Assert (assertions are inside middlewares)
        var cmd = new SumTwoNumbersCommand(5, 3);
        await executor.InvokeAsync<SumTwoNumbersCommand, int>(cmd, CancellationToken.None);
    }
}