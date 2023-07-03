using System;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.CommandExecutor;

public class VoidCommand : Command
{
    public bool IsExecuted = false;

    protected override Task ExecuteAsync(CommandContext context)
    {
        IsExecuted = true;
        return Task.CompletedTask;
    }
}

public class ReturningCommand : Command<int>
{
    protected override Task<int> ExecuteAsync(CommandContext context)
    {
        return Task.FromResult(26);
    }
}

public class SumTwoNumbersCommand : Command<int>
{
    private readonly int _a;
    private readonly int _b;

    public SumTwoNumbersCommand(int a, int b)
    {
        _a = a;
        _b = b;
    }

    protected override Task<int> ExecuteAsync(CommandContext context)
    {
        return Task.FromResult(_a + _b);
    }
}

public class SumNumbersFromSharedDataCommand : Command<int>
{
    protected override Task<int> ExecuteAsync(CommandContext context)
    {
        int n1 = (int)context.GetFromSharedData("number1")!;
        int n2 = (int)context.GetFromSharedData("number2")!;

        return Task.FromResult(n1 + n2);
    }
}

public class AssertContextCommand : Command
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

public class ExceptionThrowingCommand : Command
{
    protected override Task ExecuteAsync(CommandContext context)
    {
        throw new Exception("Test exception.");
    }
}