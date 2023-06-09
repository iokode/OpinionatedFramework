using IOKode.OpinionatedFramework.Contracts;
using IOKode.OpinionatedFramework.FileSystem;
using FileNotFoundException = IOKode.OpinionatedFramework.FileSystem.FileNotFoundException;

namespace IOKode.OpinionatedFramework.ContractImplementations.LocalFileSystem;

public class LocalDisk : IFileDisk
{
    private readonly string _basePath;

    public LocalDisk(string basePath)
    {
        _basePath = basePath;
    }

    public Task<bool> ExistsFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        string cleanPath = _ClearFilePath(filePath);
        bool exists = File.Exists(cleanPath);

        return Task.FromResult(exists);
    }

    public async Task PutFileAsync(string filePath, Stream fileContent, CancellationToken cancellationToken = default)
    {
        string cleanPath = _ClearFilePath(filePath);
        bool exists = File.Exists(cleanPath);
        if (exists)
        {
            throw new FileAlreadyExistsException(filePath);
        }

        await using var fileStream = File.Create(cleanPath);
        await fileContent.CopyToAsync(fileStream, cancellationToken);
    }

    public async Task ReplaceFileAsync(string filePath, Stream fileContent, CancellationToken cancellationToken = default)
    {
        string cleanPath = _ClearFilePath(filePath);
        bool exists = File.Exists(cleanPath);

        if (exists)
        {
            File.Delete(cleanPath);
        }

        await PutFileAsync(filePath, fileContent, cancellationToken);
    }

    public Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        string cleanPath = _ClearFilePath(filePath);
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
        string cleanPath = _ClearFilePath(filePath);
        bool exists = File.Exists(cleanPath);
        if (!exists)
        {
            throw new FileNotFoundException(filePath);
        }

        File.Delete(cleanPath);
        return Task.CompletedTask;
    }

    private string _ClearFilePath(string filePath)
    {
        return Path.Combine(_basePath, filePath);
    }
}