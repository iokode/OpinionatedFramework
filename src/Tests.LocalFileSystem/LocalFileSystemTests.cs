using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.ContractImplementations.LocalFileSystem;
using IOKode.OpinionatedFramework.Facades;
using IOKode.OpinionatedFramework.FileSystem;
using IOKode.OpinionatedFramework.FileSystem.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Directory = System.IO.Directory;
using File = System.IO.File;
using FileNotFoundException = IOKode.OpinionatedFramework.FileSystem.Exceptions.FileNotFoundException;

namespace IOKode.OpinionatedFramework.Tests.LocalFileSystem;

public class LocalFileSystemTests : IDisposable
{
    private const string DiskName = "temp";
    private readonly string basePath = Path.Combine(Path.GetTempPath(), $"localfs-{Guid.NewGuid():N}");

    public LocalFileSystemTests()
    {
        Container.Advanced.Clear();
        Directory.CreateDirectory(this.basePath);
        Container.Services.AddSingleton<IFileSystem>(_ =>
        {
            var fileSystem = new ContractImplementations.FileSystem.FileSystem();
            fileSystem.AddDisk(DiskName, new LocalDisk(this.basePath));

            return fileSystem;
        });
        Container.Initialize();
    }

    [Fact]
    public async Task CreateDownloadReplaceAndDelete_Success()
    {
        const string filename = "filename.txt";

        Assert.False(await FS.GetDisk(DiskName).ExistsFileAsync(filename));

        var uploadContent = "Test content.";
        var uploadStream = new MemoryStream(Encoding.UTF8.GetBytes(uploadContent));
        await FS.GetDisk(DiskName).PutFileAsync(filename, uploadStream);

        Assert.True(await FS.GetDisk(DiskName).ExistsFileAsync(filename));

        var file = await FS.GetDisk(DiskName).GetFileAsync(filename);
        Assert.IsType<LocalFile>(file);
        Assert.Equal(filename, file.Name);
        Assert.Equal(filename, file.FullName);
        {
            await using var fileStream = await file.OpenReadStreamAsync();
            using var reader = new StreamReader(fileStream);
            string content = await reader.ReadToEndAsync();
            Assert.Equal(uploadContent, content);
        }

        var replaceContent = "New content.";
        await FS.GetDisk(DiskName).ReplaceFileAsync(filename, new MemoryStream(Encoding.UTF8.GetBytes(replaceContent)));

        await using (var fileStream = await (await FS.GetDisk(DiskName).GetFileAsync(filename)).OpenReadStreamAsync())
        using (var reader = new StreamReader(fileStream))
        {
            Assert.Equal(replaceContent, await reader.ReadToEndAsync());
        }

        await FS.GetDisk(DiskName).DeleteFileAsync(filename);
        Assert.False(await FS.GetDisk(DiskName).ExistsFileAsync(filename));
    }

    [Fact]
    public async Task PutFile_WhenAlreadyExists_Throws()
    {
        const string fileName = "already-exists.txt";
        await FS.GetDisk(DiskName).PutFileAsync(fileName, new MemoryStream("a"u8.ToArray()));

        await Assert.ThrowsAsync<FileAlreadyExistsException>(async () =>
            await FS.GetDisk(DiskName).PutFileAsync(fileName, new MemoryStream("b"u8.ToArray())));
    }

    [Fact]
    public async Task DeleteFile_WhenNotExists_Throws()
    {
        await Assert.ThrowsAsync<FileNotFoundException>(async () =>
            await FS.GetDisk(DiskName).DeleteFileAsync("missing.txt"));
    }

    [Fact]
    public async Task ReplaceFile_ReturnsDiskRelativeRepresentation()
    {
        const string fileName = "dir/content.txt";
        string content = "Second file content.";

        var replacedFile = await FS.GetDisk(DiskName).ReplaceFileAsync(fileName, new MemoryStream(Encoding.UTF8.GetBytes(content)));
        var localFile = Assert.IsType<LocalFile>(replacedFile);

        Assert.Equal("content.txt", localFile.Name);
        Assert.Equal("dir/content.txt", localFile.FullName);
        Assert.Equal((ulong)content.Length, localFile.Size);
        Assert.True(localFile.CreationTime <= localFile.UpdateTime);

        await using var fileStream = await localFile.OpenReadStreamAsync();
        using var reader = new StreamReader(fileStream);
        Assert.Equal(content, await reader.ReadToEndAsync());
    }

    [Fact]
    public async Task PutFile_CreatesMissingDirectories()
    {
        const string relativePath = "indir/dir2/file.txt";
        await FS.GetDisk(DiskName).PutFileAsync(relativePath, new MemoryStream("content"u8.ToArray()));

        string fullPath = Path.Combine(this.basePath, "indir", "dir2", "file.txt");
        Assert.True(File.Exists(fullPath));
        Assert.Equal("content", await File.ReadAllTextAsync(fullPath));
    }

    [Fact]
    public async Task PathTraversal_IsRejected()
    {
        string escapedName = $"../{Guid.NewGuid():N}.txt";
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await FS.GetDisk(DiskName).PutFileAsync(escapedName, new MemoryStream("x"u8.ToArray())));

        string parentPath = Path.GetFullPath(Path.Combine(this.basePath, escapedName));
        Assert.False(File.Exists(parentPath));
    }

    public void Dispose()
    {
        if (Directory.Exists(this.basePath))
        {
            Directory.Delete(this.basePath, true);
        }

        Container.Advanced.Clear();
    }
}
