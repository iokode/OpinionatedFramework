using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.CommandExecutor;

public class VoidCommand : Command
{
    public bool IsExecuted = false;

    protected override Task ExecuteAsync(ICommandContext context)
    {
        IsExecuted = true;
        return Task.CompletedTask;
    }
}

public class ReturningCommand : Command<int>
{
    protected override Task<int> ExecuteAsync(ICommandContext context)
    {
        return Task.FromResult(26);
    }
}

public class SumTwoNumbersCommand(int a, int b) : Command<int>
{
    protected override Task<int> ExecuteAsync(ICommandContext context)
    {
        return Task.FromResult(a + b);
    }
}

public class SumNumbersFromSharedDataCommand : Command<int>
{
    protected override Task<int> ExecuteAsync(ICommandContext context)
    {
        int n1 = context.SharedData.GetOrDefault<int>("number1")!;
        int n2 = context.SharedData.GetOrDefault<int>("number2")!;

        return Task.FromResult(n1 + n2);
    }
}

public class AssertContextCommand : Command
{
    protected override Task ExecuteAsync(ICommandContext context)
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
    protected override Task ExecuteAsync(ICommandContext context)
    {
        throw new Exception("Test exception.");
    }
}

public class SetGivenAndFamilyNamesInSharedData(string givenName, string familyName) : Command
{
    protected override Task ExecuteAsync(ICommandContext context)
    {
        context.SharedData.Set("Given name", givenName);
        context.SharedData.Set("Family name", familyName);

        return Task.CompletedTask;
    }
}

public class RemoveGivenAndFamilyNameFromSharedData : Command
{
    protected override Task ExecuteAsync(ICommandContext context)
    {
        context.SharedData.Remove("Given name");
        context.SharedData.Remove("Family name");

        return Task.CompletedTask;
    }
}

public class AssertIvanMontillaInSharedData : Command
{
    protected override Task ExecuteAsync(ICommandContext context)
    {
        Assert.True(context.SharedData.Exists("Given name"));
        Assert.True(context.SharedData.Exists("Family name"));

        Assert.Equal("Ivan", context.SharedData.Get<string>("Given name"));
        Assert.Equal("Montilla", context.SharedData.Get<string>("Family name"));

        return Task.CompletedTask;
    }
}

public class AssertGivenNameAndFamilyNameDoestExistInSharedData : Command
{
    protected override Task ExecuteAsync(ICommandContext context)
    {
        Assert.False(context.SharedData.Exists("Given name"));
        Assert.False(context.SharedData.Exists("Family name"));

        Assert.Null(context.SharedData.GetOrDefault("Given name"));
        Assert.Null(context.SharedData.GetOrDefault<string>("Family name"));

        Assert.Throws<KeyNotFoundException>(() => context.SharedData.Get("Given name"));
        Assert.Throws<KeyNotFoundException>(() => context.SharedData.Get<string>("Family name"));

        return Task.CompletedTask;
    }
}

public class ResultingSetGivenAndFamilyNamesInSharedData(string givenName, string familyName) : Command<int>
{
    protected override Task<int> ExecuteAsync(ICommandContext context)
    {
        context.SharedData.Set("Given name", givenName);
        context.SharedData.Set("Family name", familyName);

        return Task.FromResult(0);
    }
}

public class ResultingRemoveGivenAndFamilyNameFromSharedData : Command<int>
{
    protected override Task<int> ExecuteAsync(ICommandContext context)
    {
        context.SharedData.Remove("Given name");
        context.SharedData.Remove("Family name");

        return Task.FromResult(0);
    }
}

public class ResultingAssertIvanMontillaIsInSharedData : Command<int>
{
    protected override Task<int> ExecuteAsync(ICommandContext context)
    {
        Assert.True(context.SharedData.Exists("Given name"));
        Assert.True(context.SharedData.Exists("Family name"));

        Assert.Equal("Ivan", context.SharedData.Get<string>("Given name"));
        Assert.Equal("Montilla", context.SharedData.Get<string>("Family name"));

        return Task.FromResult(0);
    }
}

public class ResultingAssertGivenNameAndFamilyNameDoestExistInSharedData : Command<int>
{
    protected override Task<int> ExecuteAsync(ICommandContext context)
    {
        Assert.False(context.SharedData.Exists("Given name"));
        Assert.False(context.SharedData.Exists("Family name"));

        Assert.Null(context.SharedData.GetOrDefault<string>("Given name"));
        Assert.Null(context.SharedData.GetOrDefault<string>("Family name"));

        Assert.Throws<KeyNotFoundException>(() => context.SharedData.Get<string>("Given name"));
        Assert.Throws<KeyNotFoundException>(() => context.SharedData.Get<string>("Family name"));

        return Task.FromResult(0);
    }
}

public class AssertGivenNameAndFamilyNameDoestExistInPipelineData : Command
{
    protected override Task ExecuteAsync(ICommandContext context)
    {
        Assert.False(context.PipelineData.Exists("Given name"));
        Assert.False(context.PipelineData.Exists("Family name"));

        Assert.Null(context.PipelineData.GetOrDefault<string>("Given name"));
        Assert.Null(context.PipelineData.GetOrDefault<string>("Family name"));

        Assert.Throws<KeyNotFoundException>(() => context.PipelineData.Get<string>("Given name"));
        Assert.Throws<KeyNotFoundException>(() => context.PipelineData.Get<string>("Family name"));

        return Task.CompletedTask;
    }
}

public class SetGivenAndFamilyNamesInPipelineData(string givenName, string familyName) : Command
{
    protected override Task ExecuteAsync(ICommandContext context)
    {
        context.PipelineData.Set("Given name", givenName);
        context.PipelineData.Set("Family name", familyName);

        return Task.CompletedTask;
    }
}

public class AssertIvanMontillaIsInPipelineData : Command
{
    protected override Task ExecuteAsync(ICommandContext context)
    {
        Assert.True(context.PipelineData.Exists("Given name"));
        Assert.True(context.PipelineData.Exists("Family name"));

        Assert.Equal("Ivan", context.PipelineData.Get<string>("Given name"));
        Assert.Equal("Montilla", context.PipelineData.Get<string>("Family name"));

        return Task.CompletedTask;
    }
}

public class AssertIvanMontillaIsNotInPipelineData : Command
{
    protected override Task ExecuteAsync(ICommandContext context)
    {
        Assert.False(context.PipelineData.Exists("Given name"));
        Assert.False(context.PipelineData.Exists("Family name"));

        return Task.CompletedTask;
    }
}