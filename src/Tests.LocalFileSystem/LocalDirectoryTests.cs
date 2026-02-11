using System;
using System.IO;
using System.Linq;
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
using DirectoryNotFoundException = IOKode.OpinionatedFramework.FileSystem.Exceptions.DirectoryNotFoundException;

namespace IOKode.OpinionatedFramework.Tests.LocalFileSystem;

public class LocalDirectoryTests : IDisposable
{
    private const string DiskName = "temp";
    private readonly string basePath = Path.Combine(Path.GetTempPath(), $"localfs-dir-{Guid.NewGuid():N}");

    public LocalDirectoryTests()
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
    public async Task CreateAndDeleteDirectory_Success()
    {
        const string directoryName = "dirname";
        var createdDirectory = await FS.GetDisk(DiskName).CreateDirectoryAsync(directoryName);
        var localDirectory = Assert.IsType<LocalDirectory>(createdDirectory);

        Assert.Equal(directoryName, localDirectory.Name);
        Assert.Equal(directoryName, localDirectory.FullName);
        Assert.True(await FS.GetDisk(DiskName).ExistsDirectoryAsync(directoryName));

        await FS.GetDisk(DiskName).DeleteDirectoryAsync(directoryName);
        Assert.False(await FS.GetDisk(DiskName).ExistsDirectoryAsync(directoryName));
    }

    [Fact]
    public async Task CreateDirectory_WhenAlreadyExists_Throws()
    {
        const string directoryName = "existing";
        await FS.GetDisk(DiskName).CreateDirectoryAsync(directoryName);

        await Assert.ThrowsAsync<DirectoryAlreadyExistsException>(async () =>
            await FS.GetDisk(DiskName).CreateDirectoryAsync(directoryName));
    }

    [Fact]
    public async Task DeleteDirectory_WhenNotExists_Throws()
    {
        await Assert.ThrowsAsync<DirectoryNotFoundException>(async () =>
            await FS.GetDisk(DiskName).DeleteDirectoryAsync("missing"));
    }

    [Fact]
    public async Task ListFiles_Success()
    {
        await FS.GetDisk(DiskName).PutFileAsync("file1.txt", new MemoryStream("x"u8.ToArray()));
        await FS.GetDisk(DiskName).PutFileAsync("file2.txt", new MemoryStream("x"u8.ToArray()));
        await FS.GetDisk(DiskName).PutFileAsync("file3.txt", new MemoryStream("x"u8.ToArray()));
        await FS.GetDisk(DiskName).PutFileAsync("dir/file1.txt", new MemoryStream("x"u8.ToArray()));
        await FS.GetDisk(DiskName).PutFileAsync("dir/nested/file2.txt", new MemoryStream("x"u8.ToArray()));

        var itemsInRoot = (await FS.GetDisk(DiskName).ListFilesAsync()).ToArray();
        Assert.Equal(3, itemsInRoot.Length);
        Assert.All(itemsInRoot, item => Assert.IsType<LocalFile>(item));
        Assert.Collection(
            itemsInRoot.Select(item => item.FullName).ToArray(),
            item => Assert.Equal("file1.txt", item),
            item => Assert.Equal("file2.txt", item),
            item => Assert.Equal("file3.txt", item));

        var itemsInDirectory = (await FS.GetDisk(DiskName).ListFilesAsync("dir")).ToArray();
        Assert.Equal(2, itemsInDirectory.Length);
        Assert.All(itemsInDirectory, item => Assert.IsType<LocalFile>(item));
        Assert.Collection(
            itemsInDirectory.Select(item => item.FullName).ToArray(),
            item => Assert.Equal("dir/file1.txt", item),
            item => Assert.Equal("dir/nested/file2.txt", item));
    }

    [Fact]
    public async Task ListFiles_WhenDirectoryMissing_Throws()
    {
        await Assert.ThrowsAsync<DirectoryNotFoundException>(async () =>
            await FS.GetDisk(DiskName).ListFilesAsync("missing"));
    }

    [Fact]
    public async Task ListDirectories_Success()
    {
        await FS.GetDisk(DiskName).CreateDirectoryAsync("dir-a");
        await FS.GetDisk(DiskName).CreateDirectoryAsync("dir-b");
        await FS.GetDisk(DiskName).PutFileAsync("file.txt", new MemoryStream(Encoding.UTF8.GetBytes("x")));

        var directories = (await FS.GetDisk(DiskName).ListDirectoriesAsync()).ToArray();

        Assert.Equal(2, directories.Length);
        Assert.All(directories, item => Assert.IsType<LocalDirectory>(item));
        Assert.Collection(
            directories.Select(directory => directory.FullName).ToArray(),
            item => Assert.Equal("dir-a", item),
            item => Assert.Equal("dir-b", item));
    }

    [Fact]
    public async Task ListDirectories_WhenDirectoryMissing_Throws()
    {
        await Assert.ThrowsAsync<DirectoryNotFoundException>(async () =>
            await FS.GetDisk(DiskName).ListDirectoriesAsync("missing"));
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
