using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Facades;
using IOKode.OpinionatedFramework.FileSystem;
using FileNotFoundException = IOKode.OpinionatedFramework.FileSystem.FileNotFoundException;
using DirectoryNotFoundException = IOKode.OpinionatedFramework.FileSystem.FileNotFoundException;

namespace IOKode.OpinionatedFramework.Contracts;

/// <summary>
/// Defines the contract for a system that manages multiple disk systems.
/// </summary>
/// <remarks>
/// This interface abstracts the operations for managing multiple disk systems,
/// such as adding, retrieving, and removing disks. It allows for handling different
/// storage options within the same application.
/// </remarks>
[AddToFacade("FS")]
public interface IFileSystem
{
    public IFileDisk GetDisk(string diskName);
}

/// <summary>
/// Defines an interface for a disk system focused on file manipulation.
/// </summary>
/// <remarks>
/// This interface abstracts the operations that can be performed on a disk,
/// such as file and directory manipulation. It is agnostic to the underlying
/// storage implementation (local, cloud, etc.)
/// </remarks>
public interface IFileDisk
{
    /// <summary>
    /// Determines whether the file at the specified path exists.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    /// <returns>True if the file exists, otherwise false.</returns>
    public Task<bool> ExistsFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the content of the specified Stream to a file. If the file already exists, an exception is thrown.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="fileContent">The content to write to the file.</param>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    /// <exception cref="FileAlreadyExistsException">Thrown when the file already exists.</exception>
    public Task PutFileAsync(string filePath, Stream fileContent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces the content of a file at the specified path with the content of the specified Stream.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="fileContent">The new content for the file.</param>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    public Task ReplaceFileAsync(string filePath, Stream fileContent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the content of the file at the specified path.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    /// <returns>A Stream representing the file content.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    public Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the file at the specified path.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether the directory at the specified path exists.
    /// </summary>
    /// <param name="directoryPath">The path to the directory.</param>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    /// <returns>True if the directory exists, otherwise false.</returns>
    Task<bool> ExistsDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new directory at the specified path. If the directory already exists, an exception is thrown.
    /// </summary>
    /// <param name="directoryPath">The path to the new directory.</param>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    /// <exception cref="DirectoryAlreadyExistsException">Thrown when the directory already exists.</exception>
    Task CreateDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the directory at the specified path.
    /// </summary>
    /// <param name="directoryPath">The path to the directory.</param>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    /// <exception cref="DirectoryNotFoundException">Thrown when the directory does not exist.</exception>
    Task DeleteDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a list of file names in the directory at the specified path.
    /// </summary>
    /// <param name="directoryPath">The path to the directory. If this parameter is null, the method retrieves files from the root directory.</param>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    /// <returns>An enumerable collection of file names in the directory.</returns>
    /// <exception cref="DirectoryNotFoundException">Thrown when the directory does not exist.</exception>
    Task<IEnumerable<string>> ListFilesAsync(string? directoryPath = null,
        CancellationToken cancellationToken = default);
}