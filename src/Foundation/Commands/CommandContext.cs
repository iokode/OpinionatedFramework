using System;
using System.Threading;

namespace IOKode.OpinionatedFramework.Commands;

public abstract class CommandContext
{
    public Type CommandType { get; protected set; }
    public CancellationToken CancellationToken { get; protected set; }
    public bool IsExecuted { get; protected set; }
    public bool HasResult { get; protected set; }
    public object? Result { get; protected set; }

    public abstract object? GetSharedData(string key);
}