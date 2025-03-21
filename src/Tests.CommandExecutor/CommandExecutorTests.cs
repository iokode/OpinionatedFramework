using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.CommandExecutor;

public class CommandExecutorTests
{
    [Fact]
    public async Task InvokeVoidCommand_Success()
    {
        // Arrange
        var executor = Helpers.CreateExecutor(_ => { });

        // Act
        var cmd = new VoidCommand();
        await executor.InvokeAsync(cmd, CancellationToken.None);

        // Assert
        Assert.True(cmd.IsExecuted);
    }

    [Fact]
    public async Task InvokeReturningCommand_Success()
    {
        // Arrange
        var executor = Helpers.CreateExecutor(_ => { });

        // Act
        var cmd = new ReturningCommand();
        int result = await executor.InvokeAsync<ReturningCommand, int>(cmd, CancellationToken.None);

        // Assert
        Assert.Equal(26, result);
    }

    [Fact]
    public async Task InvokeReturningCommandWithParameters_Success()
    {
        // Arrange
        var executor = Helpers.CreateExecutor(_ => { });

        // Act
        var cmd = new SumTwoNumbersCommand(3, 5);
        int result = await executor.InvokeAsync<SumTwoNumbersCommand, int>(cmd, CancellationToken.None);

        // Assert
        Assert.Equal(8, result);
    }

    [Fact]
    public async Task AssertCommandContext_PropertiesAreOk()
    {
        // Arrange
        var executor = Helpers.CreateExecutor(_ => { });

        // Act & Assert
        var cmd = new AssertContextCommand();
        await executor.InvokeAsync(cmd, default);
    }

    [Fact]
    public async Task SharedDataFromConstructor_IsPassedToCommand()
    {
        // Arrange
        var executor = Helpers.CreateExecutor(options =>
        {
            options.SetInitialSharedData(new Dictionary<string, object?>
            {
                { "number1", 2 },
                { "number2", 3 }
            });

        });

        // Act
        var cmd = new SumNumbersFromSharedDataCommand();
        int result = await executor.InvokeAsync<SumNumbersFromSharedDataCommand, int>(cmd, CancellationToken.None);

        // Assert
        Assert.Equal(5, result);
    }

    [Fact]
    public async Task MiddlewareUpdatesSharedData_IsPassedToCommand()
    {
        // Arrange
        var executor = Helpers.CreateExecutor(options =>
        {
            options.SetInitialSharedData(new Dictionary<string, object?>
            {
                { "number1", 2 },
                { "number2", 3 }
            });

            options.AddMiddleware<UpdateSharedDataMiddleware>();
        });

        // Act
        var cmd = new SumNumbersFromSharedDataCommand();
        int result = await executor.InvokeAsync<SumNumbersFromSharedDataCommand, int>(cmd, CancellationToken.None);

        // Assert
        Assert.Equal(6, result);
    }
}