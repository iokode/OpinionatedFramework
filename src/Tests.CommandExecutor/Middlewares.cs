using System;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.CommandExecutor;

public class UpdateSharedDataMiddleware : CommandMiddleware
{
    public override async Task ExecuteAsync(CommandContext context, InvokeNextMiddlewareDelegate nextAsync)
    {
        context.SetInSharedData("number1", 3);
        await nextAsync(context);
    }
}

public class CounterMiddleware : CommandMiddleware
{
    public static int Counter { get; set; } = 0;

    public override async Task ExecuteAsync(CommandContext context, InvokeNextMiddlewareDelegate nextAsync)
    {
        MiddlewareTests.ExecutedMiddlewares.Add("Counter");
        Counter++;
        await nextAsync(context);
    }
}

public class CounterMiddleware2 : CommandMiddleware
{
    public static int Counter { get; set; } = 0;

    public override async Task ExecuteAsync(CommandContext context, InvokeNextMiddlewareDelegate nextAsync)
    {
        MiddlewareTests.ExecutedMiddlewares.Add("Counter2");
        Counter++;
        await nextAsync(context);
    }
}

public class ExceptionHandlingMiddleware : CommandMiddleware
{
    public static bool ExceptionHandled { get; set; } = false;

    public override async Task ExecuteAsync(CommandContext context, InvokeNextMiddlewareDelegate nextAsync)
    {
        try
        {
            await nextAsync(context);
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
    public override async Task ExecuteAsync(CommandContext context, InvokeNextMiddlewareDelegate nextAsync)
    {
        Assert.Equal(typeof(SumTwoNumbersCommand), context.CommandType);
        Assert.False(context.HasResult);
        Assert.Null(context.Result);
        Assert.False(context.IsExecuted);

        await nextAsync(context);
    }
}

public class AssertContextMiddlewareAfterCommand : CommandMiddleware
{
    public override async Task ExecuteAsync(CommandContext context, InvokeNextMiddlewareDelegate nextAsync)
    {
        await nextAsync(context);

        Assert.Equal(typeof(SumTwoNumbersCommand), context.CommandType);
        Assert.True(context.HasResult);
        Assert.Equal(8, context.Result);
        Assert.True(context.IsExecuted);
    }
}