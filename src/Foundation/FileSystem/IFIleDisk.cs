using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.FileSystem.Exceptions;

namespace IOKode.OpinionatedFramework.FileSystem;

/// <summary>
/// Defines an interface for a disk system focused on file and directory manipulation.
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
    /// Writes the content of the specified Stream to a new file at the given path.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="fileContent">The content to write to the file.</param>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    /// <returns>The created File object.</returns>
    /// <exception cref="FileAlreadyExistsException">Thrown when the file already exists.</exception>
    public Task<File> PutFileAsync(string filePath, Stream fileContent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces the content of a file at the specified path with the content of the specified Stream.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="fileContent">The new content for the file.</param>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    /// <returns>The updated File object.</returns>
    public Task<File> ReplaceFileAsync(string filePath, Stream fileContent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the File object representing the file at the specified path.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    /// <returns>A File object representing the file at the given path.</returns>
    /// <exception cref="Exceptions.FileNotFoundException">Thrown when the file does not exist.</exception>
    public Task<File> GetFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the file at the specified path.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether the directory at the specified path exists.
    /// </summary>
    /// <param name="directoryPath">The path to the directory.</param>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    /// <returns>True if the directory exists, otherwise false.</returns>
    Task<bool> ExistsDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new directory at the specified path.
    /// </summary>
    /// <param name="directoryPath">The path to the new directory.</param>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    /// <returns>The created Directory object.</returns>
    /// <exception cref="DirectoryAlreadyExistsException">Thrown when the directory already exists.</exception>
    Task<Directory> CreateDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the directory at the specified path.
    /// </summary>
    /// <param name="directoryPath">The path to the directory.</param>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    public Task DeleteDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a list of File objects in the directory at the specified path.
    /// </summary>
    /// <param name="directoryPath">The path to the directory. If this parameter is null, the method retrieves files from the root directory.</param>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    /// <returns>An enumerable collection of File objects in the directory.</returns>
    /// <exception cref="Exceptions.DirectoryNotFoundException">Thrown when the directory does not exist.</exception>
    Task<IEnumerable<File>> ListFilesAsync(string? directoryPath = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a list of Directory objects in the directory at the specified path.
    /// </summary>
    /// <param name="directoryPath">The path to the directory. If this parameter is null, the method retrieves directories from the root directory.</param>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    /// <returns>An enumerable collection of Directory objects in the directory.</returns>
    Task<IEnumerable<Directory>> ListDirectoriesAsync(string? directoryPath = null,
        CancellationToken cancellationToken = default);
}