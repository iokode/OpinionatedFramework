using System;
using System.Collections.Generic;
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
        int n1 = (int)context.GetFromSharedDataOrDefault("number1")!;
        int n2 = (int)context.GetFromSharedDataOrDefault("number2")!;

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

public class SetGivenAndFamilyNamesInSharedData : Command
{
    private readonly string givenName;
    private readonly string familyName;

    public SetGivenAndFamilyNamesInSharedData(string givenName, string familyName)
    {
        this.givenName = givenName;
        this.familyName = familyName;
    }
    
    protected override Task ExecuteAsync(CommandContext context)
    {
        context.SetInSharedData("Given name", this.givenName);
        context.SetInSharedData("Family name", this.familyName);

        return Task.CompletedTask;
    }
}

public class RemoveGivenAndFamilyNameFromShareData : Command
{
    protected override Task ExecuteAsync(CommandContext context)
    {
        context.RemoveFromSharedData("Given name");
        context.RemoveFromSharedData("Family name");

        return Task.CompletedTask;
    }
}

public class AssertIvanMontillaInSharedData : Command
{
    protected override Task ExecuteAsync(CommandContext context)
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
    protected override Task ExecuteAsync(CommandContext context)
    {
        Assert.False(context.ExistsInSharedData("Given name"));
        Assert.False(context.ExistsInSharedData("Family name"));

        Assert.Null(context.GetFromSharedDataOrDefault<string>("Given name"));
        Assert.Null(context.GetFromSharedDataOrDefault<string>("Family name"));

        Assert.Throws<KeyNotFoundException>(() =>
        {
            context.GetFromSharedData<string>("Given name");
        });
        
        Assert.Throws<KeyNotFoundException>(() =>
        {
            context.GetFromSharedData<string>("Family name");
        });

        return Task.CompletedTask;
    }
}

public class ResultingSetGivenAndFamilyNamesInSharedData : Command<int>
{
    private readonly string givenName;
    private readonly string familyName;

    public ResultingSetGivenAndFamilyNamesInSharedData(string givenName, string familyName)
    {
        this.givenName = givenName;
        this.familyName = familyName;
    }
    
    protected override Task<int> ExecuteAsync(CommandContext context)
    {
        context.SetInSharedData("Given name", this.givenName);
        context.SetInSharedData("Family name", this.familyName);

        return Task.FromResult(0);
    }
}

public class ResultingRemoveGivenAndFamilyNameFromShareData : Command<int>
{
    protected override Task<int> ExecuteAsync(CommandContext context)
    {
        context.RemoveFromSharedData("Given name");
        context.RemoveFromSharedData("Family name");

        return Task.FromResult(0);
    }
}

public class ResultingAssertIvanMontillaInSharedData : Command<int>
{
    protected override Task<int> ExecuteAsync(CommandContext context)
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
    protected override Task<int> ExecuteAsync(CommandContext context)
    {
        Assert.False(context.ExistsInSharedData("Given name"));
        Assert.False(context.ExistsInSharedData("Family name"));

        Assert.Null(context.GetFromSharedDataOrDefault<string>("Given name"));
        Assert.Null(context.GetFromSharedDataOrDefault<string>("Family name"));

        Assert.Throws<KeyNotFoundException>(() =>
        {
            context.GetFromSharedData<string>("Given name");
        });
        
        Assert.Throws<KeyNotFoundException>(() =>
        {
            context.GetFromSharedData<string>("Family name");
        });

        return Task.FromResult(0);
    }
}