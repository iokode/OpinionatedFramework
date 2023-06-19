using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Ensuring;
using IOKode.OpinionatedFramework.FileSystem;
using Directory = System.IO.Directory;
using DirectoryNotFoundException = IOKode.OpinionatedFramework.FileSystem.DirectoryNotFoundException;
using File = System.IO.File;
using FileNotFoundException = IOKode.OpinionatedFramework.FileSystem.FileNotFoundException;

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

    public async Task<FileSystem.File> PutFileAsync(string filePath, Stream fileContent,
        CancellationToken cancellationToken = default)
    {
        string cleanPath = ClearFilePath(filePath);
        var file = new FileInfo(cleanPath);

        if (file.Exists)
        {
            throw new FileAlreadyExistsException(filePath);
        }

        if (!file.Directory?.Exists ?? false)
        {
            file.Directory.Create();
        }

        await using (var fileStream = file.Create())
        {
            await fileContent.CopyToAsync(fileStream, cancellationToken);
        }

        return CreateFileRepresentation(cleanPath);
    }

    public async Task<FileSystem.File> ReplaceFileAsync(string filePath, Stream fileContent,
        CancellationToken cancellationToken = default)
    {
        string cleanPath = ClearFilePath(filePath);
        bool exists = File.Exists(cleanPath);

        if (exists)
        {
            File.Delete(cleanPath);
        }

        return await PutFileAsync(filePath, fileContent, cancellationToken);
    }

    public Task<FileSystem.File> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        string cleanPath = ClearFilePath(filePath);

        if (!File.Exists(cleanPath))
        {
            throw new FileNotFoundException(filePath);
        }

        var fileRepresentation = CreateFileRepresentation(cleanPath);
        return Task.FromResult(fileRepresentation);
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

    public Task<FileSystem.Directory> CreateDirectoryAsync(string directoryPath,
        CancellationToken cancellationToken = default)
    {
        string cleanPath = ClearFilePath(directoryPath);

        if (Directory.Exists(cleanPath))
        {
            throw new DirectoryAlreadyExistsException(directoryPath);
        }

        Directory.CreateDirectory(cleanPath);

        var directoryRepresentation = CreateDirectoryRepresentation(cleanPath);
        return Task.FromResult(directoryRepresentation);
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

    public Task<IEnumerable<FileSystem.File>> ListFilesAsync(string? directoryPath = null,
        CancellationToken cancellationToken = default)
    {
        directoryPath ??= string.Empty;
        string cleanPath = ClearFilePath(directoryPath);

        if (directoryPath != string.Empty && !Directory.Exists(cleanPath))
        {
            throw new DirectoryNotFoundException(directoryPath);
        }

        var fileRepresentations = Directory.GetFiles(cleanPath).Select(CreateFileRepresentation);
        return Task.FromResult(fileRepresentations);
    }

    public Task<IEnumerable<FileSystem.Directory>> ListDirectoriesAsync(string? directoryPath = null,
        CancellationToken cancellationToken = default)
    {
        directoryPath ??= string.Empty;
        string cleanPath = ClearFilePath(directoryPath);

        if (directoryPath != string.Empty && !Directory.Exists(cleanPath))
        {
            throw new DirectoryNotFoundException(directoryPath);
        }

        var directoryRepresentations = Directory.GetDirectories(cleanPath).Select(CreateDirectoryRepresentation);
        return Task.FromResult(directoryRepresentations);
    }


    private FileSystem.File CreateFileRepresentation(string cleanPath)
    {
        var fileInfo = new FileInfo(cleanPath);
        fileInfo.Refresh();
        if (!fileInfo.Exists)
        {
            throw new ArgumentException("Trying to create a file representation of non-existent file.");
        }

        if (fileInfo.Length == 0)
        {
            using var fileStream = fileInfo.OpenRead();
            // If fileStream.Length is 0, then the file is indeed empty
            if (fileStream.Length != 0)
            {
                // The file system may not update the file size immediately after writing to the file,
                // so we loop until the file size is updated.
                while (fileInfo.Length == 0)
                {
                    Task.Delay(100).Wait();
                    fileInfo.Refresh();
                }
            }
        }

        return new LocalFile
        {
            CreationTime = fileInfo.CreationTime,
            UpdateTime = fileInfo.LastWriteTimeUtc,
            FullName = fileInfo.FullName,
            Name = fileInfo.Name,
            Size = (uint)fileInfo.Length
        };
    }

    private FileSystem.Directory CreateDirectoryRepresentation(string cleanPath)
    {
        var directoryInfo = new DirectoryInfo(cleanPath);
        if (!directoryInfo.Exists)
        {
            throw new ArgumentException("Trying to create a directory representation of non-existent directory.");
        }

        return new FileSystem.Directory()
        {
            CreationTime = directoryInfo.CreationTime,
            UpdateTime = directoryInfo.LastWriteTimeUtc,
            FullName = directoryInfo.FullName,
            Name = directoryInfo.Name
        };
    }

    private string ClearFilePath(string filePath)
    {
        return Path.Combine(_basePath, filePath);
    }
}