using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using IOKode.OpinionatedFramework.FileSystem;
using IOKode.OpinionatedFramework.FileSystem.Exceptions;
using Directory = IOKode.OpinionatedFramework.FileSystem.Directory;
using File = IOKode.OpinionatedFramework.FileSystem.File;
using FileNotFoundException = IOKode.OpinionatedFramework.FileSystem.Exceptions.FileNotFoundException;
using Object = Google.Apis.Storage.v1.Data.Object;

namespace IOKode.OpinionatedFramework.ContractImplementations.GoogleCloudStorage;

public class GoogleCloudStorageDisk : IFileDisk
{
    private readonly string _bucketName;
    private readonly StorageClient _storageClient;

    private readonly NotSupportedException _directoryException =
        new("The methods for working with directories are not applicable in GCS.");

    public GoogleCloudStorageDisk(StorageClient storageClient, string bucketName)
    {
        _bucketName = bucketName;
        _storageClient = storageClient;
    }

    public async Task<bool> ExistsFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            await _storageClient.GetObjectAsync(_bucketName, filePath, cancellationToken: cancellationToken);
            return true;
        }
        catch (Google.GoogleApiException e) when (e.Error.Code == 404)
        {
            return false;
        }
    }

    public async Task<File> PutFileAsync(Object obj, Stream fileContent, UploadObjectOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (await ExistsFileAsync(obj.Name, cancellationToken))
        {
            throw new FileAlreadyExistsException(obj.Name);
        }

        return await ReplaceFileAsync(obj, fileContent, options, cancellationToken);
    }

    public async Task<File> ReplaceFileAsync(Object obj, Stream fileContent, UploadObjectOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        obj = await _storageClient.UploadObjectAsync(obj, fileContent, options, cancellationToken);
        return CreateFileRepresentation(obj);
    }

    public async Task<File> PutFileAsync(string filePath, Stream fileContent,
        GoogleCloudStorageClass? storageClass, IDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var obj = new Object
        {
            Bucket = _bucketName,
            Name = filePath,
            StorageClass = storageClass?.ToString().ToUpper(),
            Metadata = metadata,
        };

        return await PutFileAsync(obj, fileContent, null, cancellationToken);
    }

    public async Task<File> ReplaceFileAsync(string filePath, Stream fileContent,
        GoogleCloudStorageClass? storageClass, IDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var obj = new Object
        {
            Bucket = _bucketName,
            Name = filePath,
            StorageClass = storageClass?.ToString().ToUpper(),
            Metadata = metadata,
        };

        return await ReplaceFileAsync(obj, fileContent, null, cancellationToken);
    }

    public async Task<File> PutFileAsync(string filePath, Stream fileContent,
        CancellationToken cancellationToken = default)
    {
        return await PutFileAsync(filePath, fileContent, null, null, cancellationToken);
    }

    public async Task<File> ReplaceFileAsync(string filePath, Stream fileContent,
        CancellationToken cancellationToken = default)
    {
        return await ReplaceFileAsync(filePath, fileContent, null, null, cancellationToken);
    }

    public async Task<File> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!await ExistsFileAsync(filePath, cancellationToken))
        {
            throw new FileNotFoundException(filePath);
        }

        var obj = await _storageClient.GetObjectAsync(_bucketName, filePath, cancellationToken: cancellationToken);
        return CreateFileRepresentation(obj);
    }

    public async Task<IEnumerable<File>> ListFilesAsync(string? directoryPath = null,
        CancellationToken cancellationToken = default)
    {
        bool prefixExists = !string.IsNullOrWhiteSpace(directoryPath);

        if (prefixExists && !directoryPath!.EndsWith('/'))
        {
            directoryPath += '/';
        }

        var objects = _storageClient.ListObjectsAsync(_bucketName, directoryPath);

        var files = new List<Object>();
        await foreach (var obj in objects.WithCancellation(cancellationToken))
        {
            if (prefixExists)
            {
                if (obj.Name.StartsWith(directoryPath!))
                {
                    files.Add(obj);
                }
            }
            else
            {
                if (!obj.Name.Contains('/'))
                {
                    files.Add(obj);
                }
            }
        }

        return files.Select(CreateFileRepresentation);
    }

    public async Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!await ExistsFileAsync(filePath, cancellationToken))
        {
            throw new FileNotFoundException(filePath);
        }

        await _storageClient.DeleteObjectAsync(_bucketName, filePath, cancellationToken: cancellationToken);
    }

    public Task<IEnumerable<Directory>> ListDirectoriesAsync(string? directoryPath = null,
        CancellationToken cancellationToken = default) => throw _directoryException;

    public Task<bool> ExistsDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default) =>
        throw _directoryException;

    public Task<Directory> CreateDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default) =>
        throw _directoryException;

    public Task DeleteDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default) =>
        throw _directoryException;

    private File CreateFileRepresentation(Object obj) => new GoogleCloudStorageFile(_storageClient, obj, _bucketName);
}