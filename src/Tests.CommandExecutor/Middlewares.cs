using System;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.CommandExecutor;

public class UpdateSharedDataMiddleware : CommandMiddleware
{
    public override async Task ExecuteAsync(ICommandExecutionContext executionContext, InvokeNextMiddlewareDelegate nextAsync)
    {
        executionContext.SharedData.Set("number1", 3);
        await nextAsync();
    }
}

public class CounterMiddleware : CommandMiddleware
{
    public static int Counter { get; set; } = 0;

    public override async Task ExecuteAsync(ICommandExecutionContext executionContext, InvokeNextMiddlewareDelegate nextAsync)
    {
        MiddlewareTests.ExecutedMiddlewares.Add("Counter");
        Counter++;
        await nextAsync();
    }
}

public class CounterMiddleware2 : CommandMiddleware
{
    public static int Counter { get; set; } = 0;

    public override async Task ExecuteAsync(ICommandExecutionContext executionContext, InvokeNextMiddlewareDelegate nextAsync)
    {
        MiddlewareTests.ExecutedMiddlewares.Add("Counter2");
        Counter++;
        await nextAsync();
    }
}

public class ExceptionHandlingMiddleware : CommandMiddleware
{
    public static bool ExceptionHandled { get; set; } = false;

    public override async Task ExecuteAsync(ICommandExecutionContext executionContext, InvokeNextMiddlewareDelegate nextAsync)
    {
        try
        {
            await nextAsync();
        }
        catch (Exception ex)
        {
            Assert.Equal("Test exception.", ex.Message);
            ExceptionHandled = true;
        }
    }
}

public class AssertContextMiddlewareBeforeCommand : CommandMiddleware
{
    public override async Task ExecuteAsync(ICommandExecutionContext executionContext, InvokeNextMiddlewareDelegate nextAsync)
    {
        Assert.Equal(typeof(SumTwoNumbersCommand), executionContext.CommandType);
        Assert.False(executionContext.HasResult);
        Assert.Null(executionContext.Result);
        Assert.False(executionContext.IsExecuted);

        await nextAsync();
    }
}

public class AssertContextMiddlewareAfterCommand : CommandMiddleware
{
    public override async Task ExecuteAsync(ICommandExecutionContext executionContext, InvokeNextMiddlewareDelegate nextAsync)
    {
        await nextAsync();

        Assert.Equal(typeof(SumTwoNumbersCommand), executionContext.CommandType);
        Assert.True(executionContext.HasResult);
        Assert.Equal(8, executionContext.Result);
        Assert.True(executionContext.IsExecuted);
    }
}

public class SetIvanMontillaInPipelineDataMiddleware : CommandMiddleware
{
    public override async Task ExecuteAsync(ICommandExecutionContext executionContext, InvokeNextMiddlewareDelegate nextAsync)
    {
        executionContext.PipelineData.Set("Given name", "Ivan");
        executionContext.PipelineData.Set("Family name", "Montilla");

        await nextAsync();
    }
}