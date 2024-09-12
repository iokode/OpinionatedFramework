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
        int n1 = (int)context.GetFromSharedDataOrDefault("number1")!;
        int n2 = (int)context.GetFromSharedDataOrDefault("number2")!;

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
        context.SetInSharedData("Given name", givenName);
        context.SetInSharedData("Family name", familyName);

        return Task.CompletedTask;
    }
}

public class RemoveGivenAndFamilyNameFromSharedData : Command
{
    protected override Task ExecuteAsync(ICommandContext context)
    {
        context.RemoveFromSharedData("Given name");
        context.RemoveFromSharedData("Family name");

        return Task.CompletedTask;
    }
}

public class AssertIvanMontillaInSharedData : Command
{
    protected override Task ExecuteAsync(ICommandContext context)
    {
        Assert.True(context.ExistsInSharedData("Given name"));
        Assert.True(context.ExistsInSharedData("Family name"));

        Assert.Equal("Ivan", context.GetFromSharedData<string>("Given name"));
        Assert.Equal("Montilla", context.GetFromSharedData<string>("Family name"));

        return Task.CompletedTask;
    }
}

public class AssertGivenNameAndFamilyNameDoestExistInSharedData : Command
{
    protected override Task ExecuteAsync(ICommandContext context)
    {
        Assert.False(context.ExistsInSharedData("Given name"));
        Assert.False(context.ExistsInSharedData("Family name"));

        Assert.Null(context.GetFromSharedDataOrDefault<string>("Given name"));
        Assert.Null(context.GetFromSharedDataOrDefault<string>("Family name"));

        Assert.Throws<KeyNotFoundException>(() => context.GetFromSharedData<string>("Given name"));
        Assert.Throws<KeyNotFoundException>(() => context.GetFromSharedData<string>("Family name"));

        return Task.CompletedTask;
    }
}

public class ResultingSetGivenAndFamilyNamesInSharedData(string givenName, string familyName) : Command<int>
{
    protected override Task<int> ExecuteAsync(ICommandContext context)
    {
        context.SetInSharedData("Given name", givenName);
        context.SetInSharedData("Family name", familyName);

        return Task.FromResult(0);
    }
}

public class ResultingRemoveGivenAndFamilyNameFromSharedData : Command<int>
{
    protected override Task<int> ExecuteAsync(ICommandContext context)
    {
        context.RemoveFromSharedData("Given name");
        context.RemoveFromSharedData("Family name");

        return Task.FromResult(0);
    }
}

public class ResultingAssertIvanMontillaIsInSharedData : Command<int>
{
    protected override Task<int> ExecuteAsync(ICommandContext context)
    {
        Assert.True(context.ExistsInSharedData("Given name"));
        Assert.True(context.ExistsInSharedData("Family name"));

        Assert.Equal("Ivan", context.GetFromSharedData<string>("Given name"));
        Assert.Equal("Montilla", context.GetFromSharedData<string>("Family name"));

        return Task.FromResult(0);
    }
}

public class ResultingAssertGivenNameAndFamilyNameDoestExistInSharedData : Command<int>
{
    protected override Task<int> ExecuteAsync(ICommandContext context)
    {
        Assert.False(context.ExistsInSharedData("Given name"));
        Assert.False(context.ExistsInSharedData("Family name"));

        Assert.Null(context.GetFromSharedDataOrDefault<string>("Given name"));
        Assert.Null(context.GetFromSharedDataOrDefault<string>("Family name"));

        Assert.Throws<KeyNotFoundException>(() => context.GetFromSharedData<string>("Given name"));
        Assert.Throws<KeyNotFoundException>(() => context.GetFromSharedData<string>("Family name"));

        return Task.FromResult(0);
    }
}

public class AssertGivenNameAndFamilyNameDoestExistInPipelineData : Command
{
    protected override Task ExecuteAsync(ICommandContext context)
    {
        Assert.False(context.ExistsInPipelineData("Given name"));
        Assert.False(context.ExistsInPipelineData("Family name"));

        Assert.Null(context.GetFromPipelineDataOrDefault<string>("Given name"));
        Assert.Null(context.GetFromPipelineDataOrDefault<string>("Family name"));

        Assert.Throws<KeyNotFoundException>(() => context.GetFromPipelineData<string>("Given name"));
        Assert.Throws<KeyNotFoundException>(() => context.GetFromPipelineData<string>("Family name"));

        return Task.CompletedTask;
    }
}

public class SetGivenAndFamilyNamesInPipelineData(string givenName, string familyName) : Command
{
    protected override Task ExecuteAsync(ICommandContext context)
    {
        context.SetInPipelineData("Given name", givenName);
        context.SetInPipelineData("Family name", familyName);

        return Task.CompletedTask;
    }
}

public class AssertIvanMontillaIsInPipelineData : Command
{
    protected override Task ExecuteAsync(ICommandContext context)
    {
        Assert.True(context.ExistsInPipelineData("Given name"));
        Assert.True(context.ExistsInPipelineData("Family name"));

        Assert.Equal("Ivan", context.GetFromPipelineData<string>("Given name"));
        Assert.Equal("Montilla", context.GetFromPipelineData<string>("Family name"));

        return Task.CompletedTask;
    }
}

public class AssertIvanMontillaIsNotInPipelineData : Command
{
    protected override Task ExecuteAsync(ICommandContext context)
    {
        Assert.False(context.ExistsInPipelineData("Given name"));
        Assert.False(context.ExistsInPipelineData("Family name"));

        return Task.CompletedTask;
    }
}