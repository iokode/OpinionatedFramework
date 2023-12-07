using System;
using System.Threading;

namespace IOKode.OpinionatedFramework.Commands;

/// <summary>
/// Represents the context in which a command is executed.
/// </summary>
public abstract class CommandContext
{
    /// <summary>
    /// Gets the type of the command that is being executed.
    /// </summary>
    public Type CommandType { get; protected set; } = null!;

    /// <summary>
    /// Gets a cancellation token that should be used to cancel the command execution.
    /// </summary>
    public CancellationToken CancellationToken { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether the command has been executed.
    /// </summary>
    /// <remarks>
    /// Within a command, the value of this property is always false. It is useful within a middleware.
    /// </remarks>
    public bool IsExecuted { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether the command execution has produced a result.
    /// </summary>
    /// <remarks>
    /// Within a command, the value of this property is always false. It is useful within a middleware.
    /// </remarks>
    public bool HasResult { get; protected set; }

    /// <summary>
    /// Gets the result of the command execution, if any.
    /// </summary>
    /// <remarks>
    /// Within a command, the value of this property is always null. It is useful within a middleware.
    /// </remarks>
    public object? Result { get; protected set; }

    /// <summary>
    /// Checks if a value is present in the shared data for the given key.
    /// </summary>
    /// <param name="key">The key to check for presence in the shared data.</param>
    /// <returns>Returns true if a value with the specified key exists in the shared data; otherwise, false.</returns>
    public abstract bool ExistsInSharedData(string key);

    /// <summary>
    /// Retrieves a value from the shared data.
    /// </summary>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <returns>The value associated with the specified key.</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">The key does not exists.</exception>
    public abstract object? GetFromSharedData(string key);

    /// <summary>
    /// Retrieves a value of type T from the shared data, if it exists.
    /// </summary>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <returns>The value associated with the specified key, cast to type T.</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">The key does not exists.</exception>
    public T? GetFromSharedData<T>(string key) => (T?)GetFromSharedData(key);
    
    /// <summary>
    /// Retrieves a value from the shared data, if it exists.
    /// </summary>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <returns>The value associated with the specified key, or null if the key does not exist.</returns>
    public abstract object? GetFromSharedDataOrDefault(string key);

    /// <summary>
    /// Retrieves a value of type T from the shared data, if it exists.
    /// </summary>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <returns>The value associated with the specified key, cast to type T,
    /// or null if the key does not exist or the value cannot be cast to type T.</returns>
    public T? GetFromSharedDataOrDefault<T>(string key) => (T?)GetFromSharedDataOrDefault(key);

    /// <summary>
    /// Stores a value in the shared data.
    /// </summary>
    /// <remarks>
    /// If a value with the same key already exists in the shared data, it will be replaced.
    /// </remarks>
    /// <param name="key">The key under which the value should be stored.</param>
    /// <param name="value">The value to store.</param>
    public abstract void SetInSharedData(string key, object? value);

    /// <summary>
    /// Remove a value from the shared data, if it exists.
    /// </summary>
    /// <param name="key">The key of the value to remove.</param>
    public abstract void RemoveFromSharedData(string key);
}