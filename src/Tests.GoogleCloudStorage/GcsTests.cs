using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.ContractImplementations.FileSystem;
using IOKode.OpinionatedFramework.ContractImplementations.GoogleCloudStorage;
using IOKode.OpinionatedFramework.Facades;
using IOKode.OpinionatedFramework.FileSystem;
using IOKode.OpinionatedFramework.FileSystem.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FileNotFoundException = IOKode.OpinionatedFramework.FileSystem.Exceptions.FileNotFoundException;

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
        var downloadStream = await (await FS.GetDisk(_diskname).GetFileAsync(_filename)).OpenReadStreamAsync();
        var downloadReader = new StreamReader(downloadStream);
        var downloadContent = await downloadReader.ReadToEndAsync();

        // Assert downloaded file content match uploaded content
        Assert.Equal(uploadContent, downloadContent);


        // Act - replace the recent created file
        var replaceContent = "New content.";
        var replaceStream = new MemoryStream(Encoding.UTF8.GetBytes(replaceContent));
        await FS.GetDisk(_diskname).ReplaceFileAsync(_filename, replaceStream);

        // Act - download the replaced file
        downloadStream = await (await FS.GetDisk(_diskname).GetFileAsync(_filename)).OpenReadStreamAsync();
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
        var fileContent = "Text content.";
        byte[] fileContentBytes = Encoding.UTF8.GetBytes(fileContent);
        var fileStream = new MemoryStream(fileContentBytes);
        var expectedStorageClass =
            GoogleCloudStorageHelpers.GetStorageClassFromString(
                (await _client.GetBucketAsync(_bucketName)).StorageClass);

        await EmptyBucketAsync();
        await FS.GetDisk(_diskname).PutFileAsync("file1.txt", fileStream);
        fileStream.Position = 0;
        await FS.GetDisk(_diskname).PutFileAsync("file2.txt", fileStream);
        fileStream.Position = 0;
        await FS.GetDisk(_diskname).PutFileAsync("file3.txt", fileStream);
        fileStream.Position = 0;
        await FS.GetDisk(_diskname).PutFileAsync("dir/file1.txt", fileStream);
        fileStream.Position = 0;
        await FS.GetDisk(_diskname).PutFileAsync("dir/file2.txt", fileStream);
        fileStream.Position = 0;

        // Act - List files in root
        var itemsInRoot = (await FS.GetDisk(_diskname).ListFilesAsync()).ToArray();

        // Assert three files in root
        Assert.Equal(3, itemsInRoot.Length);

        Assert.Equal("file1.txt", itemsInRoot[0].Name);
        Assert.Equal("file1.txt", itemsInRoot[0].FullName);
        Assert.Equal((ulong)13, itemsInRoot[0].Size);

        Assert.Equal("file2.txt", itemsInRoot[1].Name);
        Assert.Equal("file2.txt", itemsInRoot[1].FullName);
        Assert.Equal((ulong)13, itemsInRoot[1].Size);

        Assert.Equal("file3.txt", itemsInRoot[2].Name);
        Assert.Equal("file3.txt", itemsInRoot[2].FullName);
        Assert.Equal((ulong)13, itemsInRoot[2].Size);

        Assert.All(itemsInRoot, item =>
        {
            Assert.IsType<GoogleCloudStorageFile>(item);
            var gcsFile = (GoogleCloudStorageFile)item;
            Assert.Equal(expectedStorageClass, gcsFile.StorageClass);
            Assert.NotNull(gcsFile.Obj);
            Assert.Equal((ulong)13, gcsFile.Obj.Size);
        });

        // Act - List files in directory
        var itemsInDirectory = (await FS.GetDisk(_diskname).ListFilesAsync("dir")).ToArray();

        // Assert two files in directory
        Assert.Equal(2, itemsInDirectory.Length);

        Assert.Equal("file1.txt", itemsInDirectory[0].Name);
        Assert.Equal("dir/file1.txt", itemsInDirectory[0].FullName);
        Assert.Equal((ulong)13, itemsInDirectory[0].Size);

        Assert.Equal("file2.txt", itemsInDirectory[1].Name);
        Assert.Equal("dir/file2.txt", itemsInDirectory[1].FullName);
        Assert.Equal((ulong)13, itemsInDirectory[1].Size);

        Assert.All(itemsInDirectory, item =>
        {
            Assert.IsType<GoogleCloudStorageFile>(item);
            var gcsFile = (GoogleCloudStorageFile)item;
            Assert.Equal(expectedStorageClass, gcsFile.StorageClass);
            Assert.NotNull(gcsFile.Obj);
            Assert.Equal((ulong)13, gcsFile.Obj.Size);
        });
    }

    [Fact]
    public async Task ReplaceFile_WithCustomStorageClass()
    {
        // Arrange
        var fileContent = "Text content.";
        byte[] fileContentBytes = Encoding.UTF8.GetBytes(fileContent);
        var fileStream = new MemoryStream(fileContentBytes);

        // Act
        var disk = (GoogleCloudStorageDisk)FS.GetDisk("gcs");
        await disk.ReplaceFileAsync("customclass/standard.txt", fileStream, GoogleCloudStorageClass.Standard);
        await disk.ReplaceFileAsync("customclass/coldline.txt", fileStream, GoogleCloudStorageClass.Coldline);
        await disk.ReplaceFileAsync("customclass/nearline.txt", fileStream, GoogleCloudStorageClass.Nearline);
        await disk.ReplaceFileAsync("customclass/archive.txt", fileStream, GoogleCloudStorageClass.Archive);

        // Assert
        var standardFile = (GoogleCloudStorageFile)await disk.GetFileAsync("customclass/standard.txt");
        var coldlineFile = (GoogleCloudStorageFile)await disk.GetFileAsync("customclass/coldline.txt");
        var nearlineFile = (GoogleCloudStorageFile)await disk.GetFileAsync("customclass/nearline.txt");
        var archiveFile = (GoogleCloudStorageFile)await disk.GetFileAsync("customclass/archive.txt");

        Assert.Equal(GoogleCloudStorageClass.Standard, standardFile.StorageClass);
        Assert.Equal(GoogleCloudStorageClass.Coldline, coldlineFile.StorageClass);
        Assert.Equal(GoogleCloudStorageClass.Nearline, nearlineFile.StorageClass);
        Assert.Equal(GoogleCloudStorageClass.Archive, archiveFile.StorageClass);
    }
    
    [Fact]
    public async Task ReplaceFile_WithCustomMetadata()
    {
        // Arrange
        var fileContent = "Text content.";
        byte[] fileContentBytes = Encoding.UTF8.GetBytes(fileContent);
        var fileStream = new MemoryStream(fileContentBytes);

        // Act
        var disk = (GoogleCloudStorageDisk)FS.GetDisk("gcs");
        await disk.ReplaceFileAsync("customdata/greet.txt", fileStream, null, new Dictionary<string, string>()
        {
            {"Greeting", "Hello world!"}
        });

        // Assert
        var file = (GoogleCloudStorageFile)await disk.GetFileAsync("customdata/greet.txt");

        Assert.Equal(new Dictionary<string, string>()
        {
            {"Greeting", "Hello world!"}
        }, file.Obj.Metadata);
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
        Container.Advanced.Clear();
        _client.Dispose();
    }
}