using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using IOKode.OpinionatedFramework.ConfigureApplication;
using IOKode.OpinionatedFramework.ContractImplementations.FileSystem;
using IOKode.OpinionatedFramework.ContractImplementations.GoogleCloudStorage;
using IOKode.OpinionatedFramework.Contracts;
using IOKode.OpinionatedFramework.Facades;
using IOKode.OpinionatedFramework.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FileNotFoundException = IOKode.OpinionatedFramework.FileSystem.FileNotFoundException;

namespace Tests.GoogleCloudStorage;

/*
Prior to executing these tests, two environment variables should be provided to the test runner:
- GOOGLE_APPLICATION_CREDENTIALS
- GOOGLE_GCS_BUCKET_NAME

Please note that these integration tests make calls to Google Cloud Storage.
As a result, execution may incur charges on your Google Cloud account.
*/
public class GcsTests : IDisposable
{
    private string _bucketName = Environment.GetEnvironmentVariable("GOOGLE_GCS_BUCKET_NAME")!;
    private readonly StorageClient _client;
    private const string _diskname = "gcs";
    private const string _filename = "filename.txt";

    public GcsTests()
    {
        _client = StorageClient.Create();

        Container.Services.AddSingleton<IFileSystem>(_ =>
        {
            var fileSystem = new FileSystem();
            fileSystem.AddDisk(_diskname, new GoogleCloudStorageDisk(_client, _bucketName));

            return fileSystem;
        });
        Container.Initialize();
    }

    [Fact]
    public async Task CreateDownloadReplaceAndDelete_Success()
    {
        // NOTE: This is a complete test that uses the implementation for asserts.
        //       Other tests using the Google Storage Client are necessary.

        // Arrange - delete file if exists
        if (await FS.GetDisk(_diskname).ExistsFileAsync(_filename))
        {
            await FS.GetDisk(_diskname).DeleteFileAsync(_filename);
        }

        // Assert file does not exists
        Assert.False(await FS.GetDisk(_diskname).ExistsFileAsync(_filename));

        // Act - create and upload a file
        var uploadContent = "Test content.";
        var uploadStream = new MemoryStream(Encoding.UTF8.GetBytes(uploadContent));
        await FS.GetDisk(_diskname).PutFileAsync(_filename, uploadStream);

        // Assert file exists
        Assert.True(await FS.GetDisk(_diskname).ExistsFileAsync(_filename));

        // Act - download the file
        var downloadStream = await FS.GetDisk(_diskname).GetFileAsync(_filename);
        var downloadReader = new StreamReader(downloadStream);
        var downloadContent = await downloadReader.ReadToEndAsync();

        // Assert downloaded file content match uploaded content
        Assert.Equal(uploadContent, downloadContent);


        // Act - replace the recent created file
        var replaceContent = "New content.";
        var replaceStream = new MemoryStream(Encoding.UTF8.GetBytes(replaceContent));
        await FS.GetDisk(_diskname).ReplaceFileAsync(_filename, replaceStream);

        // Act - download the replaced file
        downloadStream = await FS.GetDisk(_diskname).GetFileAsync(_filename);
        downloadReader = new StreamReader(downloadStream);
        downloadContent = await downloadReader.ReadToEndAsync();

        // Assert downloaded file content match replaced content
        Assert.Equal(replaceContent, downloadContent);

        // Act - delete the file
        await FS.GetDisk(_diskname).DeleteFileAsync(_filename);

        // Assert file not exists
        Assert.False(await FS.GetDisk(_diskname).ExistsFileAsync(_filename));
    }

    [Fact]
    public async Task PutFile_Success()
    {
        // Arrange
        await DeleteFileIfExistsAsync();
        var fileStream = new MemoryStream("Text content."u8.ToArray());

        // Act - Put file
        await FS.GetDisk(_diskname).PutFileAsync(_filename, fileStream);

        // Assert file exists
        Assert.True(await FileExistsAsync());

        // Act & assert - Put file when exists
        await Assert.ThrowsAsync<FileAlreadyExistsException>(async () =>
            await FS.GetDisk(_diskname).PutFileAsync(_filename, fileStream));

        // Cleanup - delete file
        await DeleteFileIfExistsAsync();
    }

    [Fact]
    public async Task DeleteFile_Success()
    {
        // Arrange
        await CreateFileIfNotExists();

        // Act
        await FS.GetDisk(_diskname).DeleteFileAsync(_filename);

        // Assert file not exists
        Assert.False(await FileExistsAsync());

        // Assert throws when trying to delete not existent file
        await Assert.ThrowsAsync<FileNotFoundException>(async () =>
            await FS.GetDisk(_diskname).DeleteFileAsync(_filename));
    }

    [Fact]
    public async Task DirectoryOperations_Unsupported()
    {
        await Assert.ThrowsAsync<NotSupportedException>(async () =>
            await FS.GetDisk(_diskname).CreateDirectoryAsync("dirname"));
        await Assert.ThrowsAsync<NotSupportedException>(async () =>
            await FS.GetDisk(_diskname).DeleteDirectoryAsync("dirname"));
        await Assert.ThrowsAsync<NotSupportedException>(async () =>
            await FS.GetDisk(_diskname).ExistsDirectoryAsync("dirname"));
    }

    [Fact]
    public async Task ListFiles_Success()
    {
        // Arrange
        await EmptyBucketAsync();
        var fileStream = new MemoryStream("Text content."u8.ToArray());
        await _client.UploadObjectAsync(_bucketName, "file1.txt", null, fileStream);
        await _client.UploadObjectAsync(_bucketName, "file2.txt", null, fileStream);
        await _client.UploadObjectAsync(_bucketName, "file3.txt", null, fileStream);
        await _client.UploadObjectAsync(_bucketName, "dir/file1.txt", null, fileStream);
        await _client.UploadObjectAsync(_bucketName, "dir/file2.txt", null, fileStream);
        
        // Act - List files in root
        string[] itemsInRoot = (await FS.GetDisk(_diskname).ListFilesAsync()).ToArray();
        
        // Assert three files in root
        Assert.Equal(3, itemsInRoot.Length);
        Assert.Contains("file1.txt", itemsInRoot);
        Assert.Contains("file2.txt", itemsInRoot);
        Assert.Contains("file3.txt", itemsInRoot);
        
        // Act - List files in directory
        string[] itemsInDirectory = (await FS.GetDisk(_diskname).ListFilesAsync("dir")).ToArray();
        
        // Assert two files in root
        Assert.Equal(2, itemsInDirectory.Length);
        Assert.Contains("file1.txt", itemsInDirectory);
        Assert.Contains("file2.txt", itemsInDirectory);
    }

    private async Task<bool> FileExistsAsync()
    {
        try
        {
            await _client.GetObjectAsync(_bucketName, _filename);
            return true;
        }
        catch (Google.GoogleApiException e) when (e.Error.Code == 404)
        {
            return false;
        }
    }

    private async Task CreateFileIfNotExists()
    {
        if (!await FileExistsAsync())
        {
            await _client.UploadObjectAsync(_bucketName, _filename, null,
                new MemoryStream("File content."u8.ToArray()));
        }
    }

    private async Task DeleteFileIfExistsAsync()
    {
        if (await FileExistsAsync())
        {
            await _client.DeleteObjectAsync(_bucketName, _filename);
        }
    }

    private async Task EmptyBucketAsync()
    {
        var objects = _client.ListObjectsAsync(_bucketName);

        await foreach (var obj in objects)
        {
            await _client.DeleteObjectAsync(obj);
        }
    }

    public void Dispose()
    {
        Container.Clear();
        _client.Dispose();
    }
}