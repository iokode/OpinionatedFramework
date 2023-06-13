using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using IOKode.OpinionatedFramework.Contracts;
using IOKode.OpinionatedFramework.FileSystem;

namespace IOKode.OpinionatedFramework.ContractImplementations.GoogleCloudStorage;

public class GoogleCloudStorageDisk : IFileDisk
{
    private readonly string _bucketName;
    private readonly StorageClient _storageClient;

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

    public async Task PutFileAsync(string filePath, Stream fileContent,
        CancellationToken cancellationToken = default)
    {
        if (await ExistsFileAsync(filePath, cancellationToken))
        {
            throw new FileAlreadyExistsException(filePath);
        }

        await _storageClient.UploadObjectAsync(_bucketName, filePath, null, fileContent,
            cancellationToken: cancellationToken);
    }

    public async Task ReplaceFileAsync(string filePath, Stream fileContent,
        CancellationToken cancellationToken = default)
    {
        await _storageClient.UploadObjectAsync(_bucketName, filePath, null, fileContent,
            cancellationToken: cancellationToken);
    }

    public async Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!await ExistsFileAsync(filePath, cancellationToken))
        {
            throw new FileSystem.FileNotFoundException(filePath);
        }

        var stream = new MemoryStream();
        await _storageClient.DownloadObjectAsync(_bucketName, filePath, stream,
            cancellationToken: cancellationToken);
        stream.Position = 0;
        return stream;
    }

    public async Task<IEnumerable<string>> ListFilesAsync(string? directoryPath = null,
        CancellationToken cancellationToken = default)
    {
        bool prefixExists = !string.IsNullOrWhiteSpace(directoryPath);

        if (prefixExists && !directoryPath!.EndsWith('/'))
        {
            directoryPath += '/';
        }

        var objects = _storageClient.ListObjectsAsync(_bucketName, directoryPath);

        var files = new List<string>();
        await foreach (var obj in objects.WithCancellation(cancellationToken))
        {
            if (prefixExists)
            {
                if (obj.Name.StartsWith(directoryPath!))
                {
                    files.Add(obj.Name[directoryPath!.Length..]);
                }
            }
            else
            {
                if (!obj.Name.Contains('/'))
                {
                    files.Add(obj.Name);
                }
            }
        }

        return files;
    }

    public async Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!await ExistsFileAsync(filePath, cancellationToken))
        {
            throw new FileSystem.FileNotFoundException(filePath);
        }

        await _storageClient.DeleteObjectAsync(_bucketName, filePath, cancellationToken: cancellationToken);
    }

    public Task<bool> ExistsDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("The methods for working with directories are not applicable in GCS.");

    public Task CreateDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("The methods for working with directories are not applicable in GCS.");

    public Task DeleteDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("The methods for working with directories are not applicable in GCS.");
}