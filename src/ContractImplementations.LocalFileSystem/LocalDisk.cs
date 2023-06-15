using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Contracts;
using IOKode.OpinionatedFramework.Ensuring;
using IOKode.OpinionatedFramework.FileSystem;
using FileNotFoundException = IOKode.OpinionatedFramework.FileSystem.FileNotFoundException;
using DirectoryNotFoundException = IOKode.OpinionatedFramework.FileSystem.DirectoryNotFoundException;

namespace IOKode.OpinionatedFramework.ContractImplementations.LocalFileSystem;

public class LocalDisk : IFileDisk
{
    private readonly string _basePath;

    public LocalDisk(string basePath)
    {
        Ensure.ArgumentNotNull(basePath);
        Ensure.Boolean.IsFalse(File.Exists(basePath))
            .ElseThrowsIllegalArgument("The provided path corresponds to a file, a directory was expected.",
                nameof(basePath));

        _basePath = basePath;

        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public Task<bool> ExistsFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        string cleanPath = ClearFilePath(filePath);
        bool exists = File.Exists(cleanPath);

        return Task.FromResult(exists);
    }

    public async Task PutFileAsync(string filePath, Stream fileContent, CancellationToken cancellationToken = default)
    {
        string cleanPath = ClearFilePath(filePath);
        bool exists = File.Exists(cleanPath);
        if (exists)
        {
            throw new FileAlreadyExistsException(filePath);
        }

        await using var fileStream = File.Create(cleanPath);
        await fileContent.CopyToAsync(fileStream, cancellationToken);
    }

    public async Task ReplaceFileAsync(string filePath, Stream fileContent,
        CancellationToken cancellationToken = default)
    {
        string cleanPath = ClearFilePath(filePath);
        bool exists = File.Exists(cleanPath);

        if (exists)
        {
            File.Delete(cleanPath);
        }

        await PutFileAsync(filePath, fileContent, cancellationToken);
    }

    public Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        string cleanPath = ClearFilePath(filePath);
        bool exists = File.Exists(cleanPath);
        if (!exists)
        {
            throw new FileNotFoundException(filePath);
        }

        var fileStream = File.OpenRead(cleanPath);
        return Task.FromResult<Stream>(fileStream);
    }

    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        string cleanPath = ClearFilePath(filePath);
        bool exists = File.Exists(cleanPath);
        if (!exists)
        {
            throw new FileNotFoundException(filePath);
        }

        File.Delete(cleanPath);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        string cleanPath = ClearFilePath(directoryPath);
        bool exists = Directory.Exists(cleanPath);
        return Task.FromResult(exists);
    }

    public Task CreateDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        string cleanPath = ClearFilePath(directoryPath);

        if (Directory.Exists(cleanPath))
        {
            throw new DirectoryAlreadyExistsException(directoryPath);
        }

        Directory.CreateDirectory(cleanPath);
        return Task.CompletedTask;
    }

    public Task DeleteDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        string cleanPath = ClearFilePath(directoryPath);

        if (!Directory.Exists(cleanPath))
        {
            throw new DirectoryNotFoundException(directoryPath);
        }

        Directory.Delete(cleanPath, true);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<string>> ListFilesAsync(string? directoryPath = null,
        CancellationToken cancellationToken = default)
    {
        directoryPath ??= string.Empty;
        string cleanPath = ClearFilePath(directoryPath);

        if (directoryPath != string.Empty && !Directory.Exists(cleanPath))
        {
            throw new DirectoryNotFoundException(directoryPath);
        }

        var files = Directory.GetFiles(cleanPath).Select(fullName => fullName[(cleanPath.Length + 1)..]);
        return Task.FromResult(files);
    }

    private string ClearFilePath(string filePath)
    {
        return Path.Combine(_basePath, filePath);
    }
}