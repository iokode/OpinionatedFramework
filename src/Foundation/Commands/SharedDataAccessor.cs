using System.Collections.Generic;

namespace IOKode.OpinionatedFramework.Commands;

public interface ISharedDataAccessor
{
    /// <summary>
    /// Gets a value indicating whether a value with the specified key exists in the shared data.
    /// <returns>True if a value with the specified key exists in the shared data; otherwise, false.</returns>
    /// </summary>
    public bool Exists(string key);

    /// <summary>
    /// Retrieves a value from the shared data.
    /// </summary>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <returns>The value associated with the specified key.</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">The key does not exists.</exception>
    public object Get(string key);

    /// <summary>
    /// Retrieves a value of type T from the shared data, if it exists.
    /// </summary>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <returns>The value associated with the specified key, cast to type T.</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">The key does not exists.</exception>
    public T Get<T>(string key) => (T)Get(key);

    /// <summary>
    /// Retrieves a value from the shared data, if it exists.
    /// </summary>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <returns>The value associated with the specified key, or null if the key does not exist.</returns>
    public object? GetOrDefault(string key);

    /// <summary>
    /// Retrieves a value of type T from the shared data, if it exists.
    /// </summary>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <returns>The value associated with the specified key, cast to type T,
    /// or null if the key does not exist or the value cannot be cast to type T.</returns>
    public T? GetOrDefault<T>(string key) => (T?)GetOrDefault(key);

    /// <summary>
    /// Stores a value in the shared data.
    /// </summary>
    /// <remarks>
    /// If a value with the same key already exists in the shared data, it will be replaced.
    /// </remarks>
    /// <param name="key">The key under which the value should be stored.</param>
    /// <param name="value">The value to store.</param>
    public void Set(string key, object? value);

    /// <summary>
    /// Remove a value from the shared data, if it exists.
    /// </summary>
    /// <param name="key">The key of the value to remove.</param>
    public void Remove(string key);

    /// <summary>
    /// Converts the shared data to a readonly dictionary.
    /// </summary>
    public IReadOnlyDictionary<string, object?> ToReadonlyDictionary();
}