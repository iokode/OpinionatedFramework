using System;
using System.Threading;
using IOKode.OpinionatedFramework.Common;

namespace IOKode.OpinionatedFramework.Commands;

/// <summary>
/// Represents the context in which a command is executed.
/// </summary>
public interface ICommandExecutionContext
{
    /// <summary>
    /// Gets the type of the command that is being executed.
    /// </summary>
    public Type CommandType { get; }

    /// <summary>
    /// Gets a cancellation token that should be used to cancel the command execution.
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Gets a value indicating whether the command has been executed.
    /// </summary>
    /// <remarks>
    /// Within a command, the value of this property is always false. It is useful within a middleware.
    /// </remarks>
    public bool IsExecuted { get; }

    /// <summary>
    /// Gets a value indicating whether the command execution has produced a result.
    /// </summary>
    /// <remarks>
    /// Within a command, the value of this property is always false. It is useful within a middleware.
    /// </remarks>
    public bool HasResult { get; }

    /// <summary>
    /// Gets the result of the command execution, if any.
    /// </summary>
    /// <remarks>
    /// Within a command, the value of this property is always null. It is useful within a middleware.
    /// </remarks>
    public object? Result { get; }

    /// <summary>
    /// Global data shared between commands and middleware in the same executor.
    /// </summary>
    public ISharedDataAccessor SharedData { get; }

    /// <summary>
    /// Local data to a command and middleware in the same pipeline.
    /// </summary>
    public ISharedDataAccessor PipelineData { get; }

    /// <summary>
    /// Unique identifier of the pipeline.
    /// </summary>
    public Guid TraceID { get; }
}