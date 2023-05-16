using System;
using System.Threading;

namespace IOKode.OpinionatedFramework.Commands;

public class CommandContext
{
    public required Type CommandType { get; init; }
    public required CancellationToken CancellationToken { get; init; }
    public bool IsExecuted { get; set; }
    public bool HasResult { get; set; }
    public object? Result { get; set; }
}