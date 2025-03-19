using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.CommandExecutor;

public class VoidCommand : Command
{
    public bool IsExecuted = false;

    protected override Task ExecuteAsync(ICommandExecutionContext executionContext)
    {
        IsExecuted = true;
        return Task.CompletedTask;
    }
}

public class ReturningCommand : Command<int>
{
    protected override Task<int> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        return Task.FromResult(26);
    }
}

public class SumTwoNumbersCommand(int a, int b) : Command<int>
{
    protected override Task<int> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        return Task.FromResult(a + b);
    }
}

public class SumNumbersFromSharedDataCommand : Command<int>
{
    protected override Task<int> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        int n1 = executionContext.SharedData.GetOrDefault<int>("number1")!;
        int n2 = executionContext.SharedData.GetOrDefault<int>("number2")!;

        return Task.FromResult(n1 + n2);
    }
}

public class AssertContextCommand : Command
{
    protected override Task ExecuteAsync(ICommandExecutionContext executionContext)
    {
        Assert.False(executionContext.IsExecuted);
        Assert.Equal(typeof(AssertContextCommand), executionContext.CommandType);
        Assert.False(executionContext.HasResult);
        Assert.Null(executionContext.Result);

        return Task.CompletedTask;
    }
}

public class ExceptionThrowingCommand : Command
{
    protected override Task ExecuteAsync(ICommandExecutionContext executionContext)
    {
        throw new Exception("Test exception.");
    }
}

public class SetGivenAndFamilyNamesInSharedData(string givenName, string familyName) : Command
{
    protected override Task ExecuteAsync(ICommandExecutionContext executionContext)
    {
        executionContext.SharedData.Set("Given name", givenName);
        executionContext.SharedData.Set("Family name", familyName);

        return Task.CompletedTask;
    }
}

public class RemoveGivenAndFamilyNameFromSharedData : Command
{
    protected override Task ExecuteAsync(ICommandExecutionContext executionContext)
    {
        executionContext.SharedData.Remove("Given name");
        executionContext.SharedData.Remove("Family name");

        return Task.CompletedTask;
    }
}

public class AssertIvanMontillaInSharedData : Command
{
    protected override Task ExecuteAsync(ICommandExecutionContext executionContext)
    {
        Assert.True(executionContext.SharedData.Exists("Given name"));
        Assert.True(executionContext.SharedData.Exists("Family name"));

        Assert.Equal("Ivan", executionContext.SharedData.Get<string>("Given name"));
        Assert.Equal("Montilla", executionContext.SharedData.Get<string>("Family name"));

        return Task.CompletedTask;
    }
}

public class AssertGivenNameAndFamilyNameDoestExistInSharedData : Command
{
    protected override Task ExecuteAsync(ICommandExecutionContext executionContext)
    {
        Assert.False(executionContext.SharedData.Exists("Given name"));
        Assert.False(executionContext.SharedData.Exists("Family name"));

        Assert.Null(executionContext.SharedData.GetOrDefault("Given name"));
        Assert.Null(executionContext.SharedData.GetOrDefault<string>("Family name"));

        Assert.Throws<KeyNotFoundException>(() => executionContext.SharedData.Get("Given name"));
        Assert.Throws<KeyNotFoundException>(() => executionContext.SharedData.Get<string>("Family name"));

        return Task.CompletedTask;
    }
}

public class ResultingSetGivenAndFamilyNamesInSharedData(string givenName, string familyName) : Command<int>
{
    protected override Task<int> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        executionContext.SharedData.Set("Given name", givenName);
        executionContext.SharedData.Set("Family name", familyName);

        return Task.FromResult(0);
    }
}

public class ResultingRemoveGivenAndFamilyNameFromSharedData : Command<int>
{
    protected override Task<int> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        executionContext.SharedData.Remove("Given name");
        executionContext.SharedData.Remove("Family name");

        return Task.FromResult(0);
    }
}

public class ResultingAssertIvanMontillaIsInSharedData : Command<int>
{
    protected override Task<int> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        Assert.True(executionContext.SharedData.Exists("Given name"));
        Assert.True(executionContext.SharedData.Exists("Family name"));

        Assert.Equal("Ivan", executionContext.SharedData.Get<string>("Given name"));
        Assert.Equal("Montilla", executionContext.SharedData.Get<string>("Family name"));

        return Task.FromResult(0);
    }
}

public class ResultingAssertGivenNameAndFamilyNameDoestExistInSharedData : Command<int>
{
    protected override Task<int> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        Assert.False(executionContext.SharedData.Exists("Given name"));
        Assert.False(executionContext.SharedData.Exists("Family name"));

        Assert.Null(executionContext.SharedData.GetOrDefault<string>("Given name"));
        Assert.Null(executionContext.SharedData.GetOrDefault<string>("Family name"));

        Assert.Throws<KeyNotFoundException>(() => executionContext.SharedData.Get<string>("Given name"));
        Assert.Throws<KeyNotFoundException>(() => executionContext.SharedData.Get<string>("Family name"));

        return Task.FromResult(0);
    }
}

public class AssertGivenNameAndFamilyNameDoestExistInPipelineData : Command
{
    protected override Task ExecuteAsync(ICommandExecutionContext executionContext)
    {
        Assert.False(executionContext.PipelineData.Exists("Given name"));
        Assert.False(executionContext.PipelineData.Exists("Family name"));

        Assert.Null(executionContext.PipelineData.GetOrDefault<string>("Given name"));
        Assert.Null(executionContext.PipelineData.GetOrDefault<string>("Family name"));

        Assert.Throws<KeyNotFoundException>(() => executionContext.PipelineData.Get<string>("Given name"));
        Assert.Throws<KeyNotFoundException>(() => executionContext.PipelineData.Get<string>("Family name"));

        return Task.CompletedTask;
    }
}

public class SetGivenAndFamilyNamesInPipelineData(string givenName, string familyName) : Command
{
    protected override Task ExecuteAsync(ICommandExecutionContext executionContext)
    {
        executionContext.PipelineData.Set("Given name", givenName);
        executionContext.PipelineData.Set("Family name", familyName);

        return Task.CompletedTask;
    }
}

public class AssertIvanMontillaIsInPipelineData : Command
{
    protected override Task ExecuteAsync(ICommandExecutionContext executionContext)
    {
        Assert.True(executionContext.PipelineData.Exists("Given name"));
        Assert.True(executionContext.PipelineData.Exists("Family name"));

        Assert.Equal("Ivan", executionContext.PipelineData.Get<string>("Given name"));
        Assert.Equal("Montilla", executionContext.PipelineData.Get<string>("Family name"));

        return Task.CompletedTask;
    }
}

public class AssertIvanMontillaIsNotInPipelineData : Command
{
    protected override Task ExecuteAsync(ICommandExecutionContext executionContext)
    {
        Assert.False(executionContext.PipelineData.Exists("Given name"));
        Assert.False(executionContext.PipelineData.Exists("Family name"));

        return Task.CompletedTask;
    }
}