using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Ensuring;
using IOKode.OpinionatedFramework.FileSystem;
using IOKode.OpinionatedFramework.FileSystem.Exceptions;
using NodaTime;
using Directory = System.IO.Directory;
using DirectoryNotFoundException = IOKode.OpinionatedFramework.FileSystem.Exceptions.DirectoryNotFoundException;
using File = System.IO.File;
using FileNotFoundException = IOKode.OpinionatedFramework.FileSystem.Exceptions.FileNotFoundException;

namespace IOKode.OpinionatedFramework.ContractImplementations.LocalFileSystem;

/// <summary>
/// Local filesystem implementation of <see cref="IFileDisk"/>.
/// </summary>
/// <remarks>
/// All paths are resolved under the configured base path and rejected when they
/// attempt to escape that base directory.
/// </remarks>
public class LocalDisk : IFileDisk
{
    private readonly string basePath;
    private readonly string basePathWithSeparator;
    private readonly StringComparison pathComparison;

    /// <summary>
    /// Creates a local disk rooted at the given base path.
    /// </summary>
    /// <param name="basePath">Base directory for all file and directory operations.</param>
    public LocalDisk(string basePath)
    {
        Ensure.ArgumentNotNull(basePath);
        Ensure.Boolean.IsFalse(File.Exists(basePath))
            .ElseThrowsIllegalArgument("The provided path corresponds to a file, a directory was expected.", nameof(basePath));

        this.basePath = NormalizeBasePath(basePath);
        this.basePathWithSeparator = this.basePath.EndsWith(Path.DirectorySeparatorChar) ? this.basePath : this.basePath + Path.DirectorySeparatorChar;
        this.pathComparison = DetectPathComparison(this.basePath);

        if (!Directory.Exists(this.basePath))
        {
            Directory.CreateDirectory(this.basePath);
        }
    }

    /// <inheritdoc />
    public Task<bool> ExistsFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string resolvedPath = this.ResolvePath(filePath, nameof(filePath));
        bool exists = File.Exists(resolvedPath);
        return Task.FromResult(exists);
    }

    /// <inheritdoc />
    public async Task<FileSystem.File> PutFileAsync(string filePath, Stream fileContent,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string resolvedPath = this.ResolvePath(filePath, nameof(filePath));
        var file = new FileInfo(resolvedPath);

        if (file.Exists)
        {
            throw new FileAlreadyExistsException(filePath);
        }

        if (!file.Directory?.Exists ?? false)
        {
            file.Directory?.Create();
        }

        await using (var fileStream = file.Create())
        {
            await fileContent.CopyToAsync(fileStream, cancellationToken);
        }

        return this.CreateFileRepresentation(resolvedPath);
    }

    /// <inheritdoc />
    public async Task<FileSystem.File> ReplaceFileAsync(string filePath, Stream fileContent,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string resolvedPath = this.ResolvePath(filePath, nameof(filePath));

        if (File.Exists(resolvedPath))
        {
            File.Delete(resolvedPath);
        }

        return await this.PutFileAsync(filePath, fileContent, cancellationToken);
    }

    /// <inheritdoc />
    public Task<FileSystem.File> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string resolvedPath = this.ResolvePath(filePath, nameof(filePath));

        if (!File.Exists(resolvedPath))
        {
            throw new FileNotFoundException(filePath);
        }

        var fileRepresentation = this.CreateFileRepresentation(resolvedPath);
        return Task.FromResult(fileRepresentation);
    }

    /// <inheritdoc />
    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string resolvedPath = this.ResolvePath(filePath, nameof(filePath));

        if (!File.Exists(resolvedPath))
        {
            throw new FileNotFoundException(filePath);
        }

        File.Delete(resolvedPath);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> ExistsDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string resolvedPath = this.ResolvePath(directoryPath, nameof(directoryPath));
        bool exists = Directory.Exists(resolvedPath);
        return Task.FromResult(exists);
    }

    /// <inheritdoc />
    public Task<FileSystem.Directory> CreateDirectoryAsync(string directoryPath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string resolvedPath = this.ResolvePath(directoryPath, nameof(directoryPath));

        if (Directory.Exists(resolvedPath))
        {
            throw new DirectoryAlreadyExistsException(directoryPath);
        }

        Directory.CreateDirectory(resolvedPath);
        return Task.FromResult(this.CreateDirectoryRepresentation(resolvedPath));
    }

    /// <inheritdoc />
    public Task DeleteDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string resolvedPath = this.ResolvePath(directoryPath, nameof(directoryPath));

        if (!Directory.Exists(resolvedPath))
        {
            throw new DirectoryNotFoundException(directoryPath);
        }

        Directory.Delete(resolvedPath, true);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IEnumerable<FileSystem.File>> ListFilesAsync(string? directoryPath = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        directoryPath ??= string.Empty;
        string resolvedPath = this.ResolvePath(directoryPath, nameof(directoryPath), allowEmpty: true);

        if (directoryPath != string.Empty && !Directory.Exists(resolvedPath))
        {
            throw new DirectoryNotFoundException(directoryPath);
        }

        var searchOption = directoryPath == string.Empty
            ? SearchOption.TopDirectoryOnly
            : SearchOption.AllDirectories;

        var fileRepresentations = Directory
            .GetFiles(resolvedPath, "*", searchOption)
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(this.CreateFileRepresentation);

        return Task.FromResult(fileRepresentations);
    }

    /// <inheritdoc />
    public Task<IEnumerable<FileSystem.Directory>> ListDirectoriesAsync(string? directoryPath = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        directoryPath ??= string.Empty;
        string resolvedPath = this.ResolvePath(directoryPath, nameof(directoryPath), allowEmpty: true);

        if (directoryPath != string.Empty && !Directory.Exists(resolvedPath))
        {
            throw new DirectoryNotFoundException(directoryPath);
        }

        var directoryRepresentations = Directory
            .GetDirectories(resolvedPath)
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(this.CreateDirectoryRepresentation);

        return Task.FromResult(directoryRepresentations);
    }

    /// <summary>
    /// Creates a local file abstraction for a physical file path.
    /// </summary>
    /// <param name="pathOnDisk">Absolute path of the file in the local filesystem.</param>
    /// <returns>A <see cref="LocalFile"/> instance describing the file.</returns>
    private FileSystem.File CreateFileRepresentation(string pathOnDisk)
    {
        var fileInfo = new FileInfo(pathOnDisk);
        fileInfo.Refresh();
        if (!fileInfo.Exists)
        {
            throw new ArgumentException("Trying to create a file representation of non-existent file.");
        }

        return new LocalFile
        {
            CreationTime = Instant.FromDateTimeUtc(fileInfo.CreationTimeUtc),
            UpdateTime = Instant.FromDateTimeUtc(fileInfo.LastWriteTimeUtc),
            Name = fileInfo.Name,
            FullName = this.GetRelativePath(pathOnDisk),
            Size = (ulong)fileInfo.Length,
            PathOnDisk = fileInfo.FullName
        };
    }

    /// <summary>
    /// Creates a local directory abstraction for a physical directory path.
    /// </summary>
    /// <param name="pathOnDisk">Absolute path of the directory in the local filesystem.</param>
    /// <returns>A <see cref="LocalDirectory"/> instance describing the directory.</returns>
    private FileSystem.Directory CreateDirectoryRepresentation(string pathOnDisk)
    {
        var directoryInfo = new DirectoryInfo(pathOnDisk);
        if (!directoryInfo.Exists)
        {
            throw new ArgumentException("Trying to create a directory representation of non-existent directory.");
        }

        return new LocalDirectory
        {
            CreationTime = Instant.FromDateTimeUtc(directoryInfo.CreationTimeUtc),
            UpdateTime = Instant.FromDateTimeUtc(directoryInfo.LastWriteTimeUtc),
            Name = directoryInfo.Name,
            FullName = this.GetRelativePath(pathOnDisk),
            PathOnDisk = directoryInfo.FullName
        };
    }

    /// <summary>
    /// Normalizes a base path to an absolute path and trims trailing separators when possible.
    /// </summary>
    /// <param name="basePath">Base path provided to the disk constructor.</param>
    /// <returns>The normalized absolute base path.</returns>
    private static string NormalizeBasePath(string basePath)
    {
        string fullPath = Path.GetFullPath(basePath);
        string? root = Path.GetPathRoot(fullPath);

        if (root is not null && string.Equals(fullPath, root, StringComparison.OrdinalIgnoreCase))
        {
            return fullPath;
        }

        return fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    /// <summary>
    /// Resolves and validates a path to ensure it remains inside the configured base path.
    /// </summary>
    /// <param name="path">Relative path provided by the caller.</param>
    /// <param name="paramName">Caller parameter name used for exception metadata.</param>
    /// <param name="allowEmpty">Whether empty paths are allowed and mapped to disk root.</param>
    /// <returns>The validated absolute path on disk.</returns>
    private string ResolvePath(string path, string paramName, bool allowEmpty = false)
    {
        Ensure.ArgumentNotNull(path);

        if (!allowEmpty && string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be empty.", paramName);
        }

        string combinedPath = string.IsNullOrWhiteSpace(path)
            ? this.basePath
            : Path.Combine(this.basePath, path);

        string resolvedPath = Path.GetFullPath(combinedPath);

        if (!string.Equals(resolvedPath, this.basePath, this.pathComparison) &&
            !resolvedPath.StartsWith(this.basePathWithSeparator, this.pathComparison))
        {
            throw new ArgumentException("Path must be inside disk base path.", paramName);
        }

        return resolvedPath;
    }

    /// <summary>
    /// Converts an absolute path on the disk into a normalized disk-relative path.
    /// </summary>
    /// <param name="pathOnDisk">Absolute path on disk.</param>
    /// <returns>Disk-relative path using forward slashes.</returns>
    private string GetRelativePath(string pathOnDisk)
    {
        string relativePath = Path.GetRelativePath(this.basePath, pathOnDisk);
        return relativePath.Replace('\\', '/');
    }

    /// <summary>
    /// Detects whether the filesystem mounted at the base path is case-insensitive.
    /// </summary>
    /// <remarks>
    /// This is implemented as a probe on the target filesystem instead of relying on
    /// operating-system detection because case sensitivity is a filesystem or mount
    /// characteristic, not an OS characteristic. The same OS can expose different
    /// behaviors depending on the backing volume (for example, case-insensitive or
    /// case-sensitive APFS variants, network shares, or mounted external drives).
    /// Probing the configured base path gives the comparison mode that matches the
    /// actual disk used by this <see cref="LocalDisk"/>.
    /// </remarks>
    /// <param name="basePath">Base path used by this disk.</param>
    /// <returns>The <see cref="StringComparison"/> mode for path comparisons.</returns>
    private static StringComparison DetectPathComparison(string basePath)
    {
        string fileName = $".fs-case-check-{Guid.NewGuid():N}.tmp";
        string lowerFileName = fileName.ToLowerInvariant();
        string upperFileName = fileName.ToUpperInvariant();
        string lowerPath = Path.Combine(basePath, lowerFileName);
        string upperPath = Path.Combine(basePath, upperFileName);

        try
        {
            using var _ = File.Create(lowerPath);
            bool ignoreCase = File.Exists(upperPath);
            return ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        }
        catch (UnauthorizedAccessException)
        {
            return StringComparison.Ordinal;
        }
        catch (IOException)
        {
            return StringComparison.Ordinal;
        }
        finally
        {
            try
            {
                if (File.Exists(lowerPath))
                {
                    File.Delete(lowerPath);
                }
            }
            catch
            {
            }

            try
            {
                if (File.Exists(upperPath))
                {
                    File.Delete(upperPath);
                }
            }
            catch
            {
            }
        }
    }
}
