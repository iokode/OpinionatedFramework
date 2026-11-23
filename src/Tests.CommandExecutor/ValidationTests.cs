using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.CommandExecutor;

public class ValidationTests
{
    private static readonly AsyncLocal<List<string>> steps = new();

    [Fact]
    public async Task InvalidCommand_ThrowsAndIsNeitherPreparedNorExecuted()
    {
        // Arrange
        steps.Value = [];
        var executor = Helpers.CreateExecutor(_ => { });
        var command = new RecordingCommand { Name = "" };

        // Act
        var exception = await Assert.ThrowsAsync<InvalidInputException>(
            () => executor.InvokeAsync(command, CancellationToken.None));

        // Assert
        var error = Assert.IsType<PathValidationError>(Assert.Single(exception.Errors));
        Assert.Equal("Name", error.Path.ToString());
        Assert.Equal(["validate"], steps.Value);
    }

    [Fact]
    public async Task InvalidCommandWithResult_ThrowsAndIsNeitherPreparedNorExecuted()
    {
        // Arrange
        steps.Value = [];
        var executor = Helpers.CreateExecutor(_ => { });
        var command = new RecordingResultCommand { Name = "" };

        // Act
        var exception = await Assert.ThrowsAsync<InvalidInputException>(
            () => executor.InvokeAsync<RecordingResultCommand, int>(command, CancellationToken.None));

        // Assert
        var error = Assert.IsType<PathValidationError>(Assert.Single(exception.Errors));
        Assert.Equal("Name", error.Path.ToString());
        Assert.Equal(["validate"], steps.Value);
    }

    [Fact]
    public async Task ValidCommand_IsValidatedAfterTheMiddlewaresAndBeforeBeingPrepared()
    {
        // Arrange
        steps.Value = [];
        var executor = Helpers.CreateExecutor(options => options.AddMiddleware<RecordingMiddleware>());
        var command = new RecordingCommand { Name = "Ana" };

        // Act
        await executor.InvokeAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(["middleware", "validate", "prepare", "execute"], steps.Value);
    }

    [Fact]
    public async Task ValidCommandWithResult_IsValidatedAfterTheMiddlewaresAndBeforeBeingPrepared()
    {
        // Arrange
        steps.Value = [];
        var executor = Helpers.CreateExecutor(options => options.AddMiddleware<RecordingMiddleware>());
        var command = new RecordingResultCommand { Name = "Ana" };

        // Act
        var result = await executor.InvokeAsync<RecordingResultCommand, int>(command, CancellationToken.None);

        // Assert
        Assert.Equal(7, result);
        Assert.Equal(["middleware", "validate", "prepare", "execute"], steps.Value);
    }

    [Fact]
    public async Task InvalidCommand_IsNotValidatedWhenAMiddlewareDoesNotContinue()
    {
        // Arrange
        steps.Value = [];
        var executor = Helpers.CreateExecutor(options => options.AddMiddleware<StoppingMiddleware>());
        var command = new RecordingCommand { Name = "" };

        // Act
        await executor.InvokeAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(["stop"], steps.Value);
    }

    private class RecordingMiddleware : CommandMiddleware
    {
        public override async Task ExecuteAsync(ICommandExecutionContext executionContext, InvokeNextMiddlewareDelegate nextAsync)
        {
            steps.Value!.Add("middleware");
            await nextAsync();
        }
    }

    private class StoppingMiddleware : CommandMiddleware
    {
        public override Task ExecuteAsync(ICommandExecutionContext executionContext, InvokeNextMiddlewareDelegate nextAsync)
        {
            steps.Value!.Add("stop");
            return Task.CompletedTask;
        }
    }

    private class RecordingCommand : Command
    {
        public required string Name { get; init; }

        public override async Task ValidateAsync(ValidationErrors errors, CancellationToken cancellationToken)
        {
            steps.Value!.Add("validate");
            await ValidateNestedInputsAsync(errors, cancellationToken);
            if (string.IsNullOrEmpty(Name))
            {
                errors.Add(nameof(Name), "A name is required.");
            }
        }

        protected override Task PrepareAsync(ICommandExecutionContext executionContext)
        {
            steps.Value!.Add("prepare");
            return Task.CompletedTask;
        }

        protected override Task ExecuteAsync(ICommandExecutionContext executionContext)
        {
            steps.Value!.Add("execute");
            return Task.CompletedTask;
        }
    }

    private class RecordingResultCommand : Command<int>
    {
        public required string Name { get; init; }

        public override async Task ValidateAsync(ValidationErrors errors, CancellationToken cancellationToken)
        {
            steps.Value!.Add("validate");
            await ValidateNestedInputsAsync(errors, cancellationToken);
            if (string.IsNullOrEmpty(Name))
            {
                errors.Add(nameof(Name), "A name is required.");
            }
        }

        protected override Task PrepareAsync(ICommandExecutionContext executionContext)
        {
            steps.Value!.Add("prepare");
            return Task.CompletedTask;
        }

        protected override Task<int> ExecuteAsync(ICommandExecutionContext executionContext)
        {
            steps.Value!.Add("execute");
            return Task.FromResult(7);
        }
    }
}
