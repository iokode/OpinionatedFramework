using System;
using System.Collections.Generic;
using System.Threading;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Common;
using IOKode.OpinionatedFramework.Internals;

namespace IOKode.OpinionatedFramework.ContractImplementations.CommandExecutor;

internal class SettableCommandExecutionContext : ICommandExecutionContext
{
    private readonly Dictionary<string, object?> sharedData;
    private readonly Dictionary<string, object?> pipelineData = new();
    private readonly ISharedDataAccessor pipelineDataAccessor;
    private readonly Guid traceID;
    private ISharedDataAccessor sharedDataAccessor;

    public ISharedDataAccessor SharedData => this.sharedDataAccessor;
    public ISharedDataAccessor PipelineData => this.pipelineDataAccessor;

    public Guid TraceID => this.traceID;
    public Type CommandType { get; private init; } = null!;
    public CancellationToken CancellationToken { get; set; }
    public bool IsExecuted { get; private set; }
    public bool HasResult { get; private set; }
    public object? Result { get; private set; }

    private SettableCommandExecutionContext(Dictionary<string, object?> initialSharedData)
    {
        this.sharedData = initialSharedData;
        this.sharedDataAccessor = new DictionarySharedDataAccessor(this.sharedData);
        this.pipelineDataAccessor = new DictionarySharedDataAccessor(this.pipelineData);
        this.traceID = Guid.NewGuid();
    }

    public void SetAsExecuted() => IsExecuted = true;

    public void SetResult(object? result)
    {
        HasResult = true;
        Result = result;
    }

    public static SettableCommandExecutionContext Create(Type commandType, Dictionary<string, object?> initialSharedData,
        CancellationToken cancellationToken)
    {
        var context = new SettableCommandExecutionContext(initialSharedData)
        {
            CommandType = commandType,
            CancellationToken = cancellationToken,
        };

        context.SetSharedDataAccessor();

        return context;
    }

    private void SetSharedDataAccessor()
    {
        this.sharedDataAccessor = new DictionarySharedDataAccessor(this.sharedData);
    }
}