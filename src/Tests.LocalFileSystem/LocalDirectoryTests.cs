using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.ContractImplementations.LocalFileSystem;
using IOKode.OpinionatedFramework.Facades;
using IOKode.OpinionatedFramework.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace IOKode.OpinionatedFramework.Tests.LocalFileSystem;

public class LocalDirectoryTests : IDisposable
{
    private const string _diskname = "temp";
    private const string _directoryname = "dirname";

    public LocalDirectoryTests()
    {
        Container.Advanced.Clear();
        Container.Services.AddSingleton<IFileSystem>(_ =>
        {
            var fileSystem = new ContractImplementations.FileSystem.FileSystem();
            fileSystem.AddDisk(_diskname, new LocalDisk(Path.GetTempPath()));

            return fileSystem;
        });

        Container.Initialize();
    }

    [Fact]
    public async Task CreateAndDeleteDirectory_Success()
    {
        // Arrange
        DeleteDirectory();

        // Assert - directory does not exist initially
        Assert.False(Directory.Exists(Path.Combine(Path.GetTempPath(), _directoryname)));

        // Act - create directory
        await FS.GetDisk(_diskname).CreateDirectoryAsync(_directoryname);

        // Assert - directory now exists
        Assert.True(Directory.Exists(Path.Combine(Path.GetTempPath(), _directoryname)));

        // Act - delete directory
        await FS.GetDisk(_diskname).DeleteDirectoryAsync(_directoryname);

        // Assert - directory no longer exists
        Assert.False(Directory.Exists(Path.Combine(Path.GetTempPath(), _directoryname)));
    }

    [Fact]
    public async Task PutFileInDirectory_Success()
    {
        // Arrange
        const string filename = "tempfile.txt";
        DeleteDirectory();

        // Act - create directory and file within it
        await FS.GetDisk(_diskname).CreateDirectoryAsync(_directoryname);

        await using var file = new MemoryStream();
        await using var writer = new StreamWriter(file);
        await writer.WriteAsync("Text content.");
        await writer.FlushAsync();
        file.Position = 0;

        await FS.GetDisk(_diskname).PutFileAsync(Path.Combine(_directoryname, filename), file);

        // Assert file exists in directory
        Assert.True(Directory.Exists(Path.Combine(Path.GetTempPath(), _directoryname)));
        Assert.True(File.Exists(Path.Combine(Path.GetTempPath(), _directoryname, filename)));

        // Act - Delete directory
        await FS.GetDisk(_diskname).DeleteDirectoryAsync(_directoryname);

        // Assert directory doesn't exists.
        Assert.False(Directory.Exists(Path.Combine(Path.GetTempPath(), _directoryname)));
        Assert.False(File.Exists(Path.Combine(Path.GetTempPath(), _directoryname, filename)));
    }

    [Fact]
    public async Task ExistsDirectoryAsync_Success()
    {
        // Arrange
        DeleteDirectory();

        // Assert directory does not exist initially
        Assert.False(Directory.Exists(Path.Combine(Path.GetTempPath(), _directoryname)));

        // Act - create directory
        await FS.GetDisk(_diskname).CreateDirectoryAsync(_directoryname);

        // Assert directory exists with FileSystem API
        Assert.True(await FS.GetDisk(_diskname).ExistsDirectoryAsync(_directoryname));

        // Act - delete directory
        await FS.GetDisk(_diskname).DeleteDirectoryAsync(_directoryname);

        // Assert directory does not exists with FileSystem API
        Assert.False(await FS.GetDisk(_diskname).ExistsDirectoryAsync(_directoryname));
    }

    [Fact]
    public async Task ListFiles_Success()
    {
        // Arrange
        DeleteDirectory();
        Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), _directoryname));
        File.CreateText(Path.Combine(Path.GetTempPath(), _directoryname, "file1.txt"));
        File.CreateText(Path.Combine(Path.GetTempPath(), _directoryname, "file2.txt"));
        File.CreateText(Path.Combine(Path.GetTempPath(), _directoryname, "file3.txt"));
        Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), _directoryname, "dir"));
        File.CreateText(Path.Combine(Path.GetTempPath(), _directoryname, "dir", "file1.txt"));
        File.CreateText(Path.Combine(Path.GetTempPath(), _directoryname, "dir", "file2.txt"));

        {
            // Act - List files in root
            var itemsInRoot = (await FS.GetDisk(_diskname).ListFilesAsync(_directoryname)).ToArray();

            // Assert three files in root
            string[] names = itemsInRoot.Select(item => item.Name).ToArray();
            Assert.Equal(3, itemsInRoot.Length);
            Assert.Contains("file1.txt", names);
            Assert.Contains("file2.txt", names);
            Assert.Contains("file3.txt", names);
        }

        {
            // Act - List files in directory
            var itemsInDirectory =
                (await FS.GetDisk(_diskname).ListFilesAsync(_directoryname + "/dir")).ToArray();

            // Assert two files in root
            string[] names = itemsInDirectory.Select(item => item.Name).ToArray();
            Assert.Equal(2, itemsInDirectory.Length);
            Assert.Contains("file1.txt", names);
            Assert.Contains("file2.txt", names);
        }
    }

    [Fact]
    public async Task ListDirectories_Success()
    {
        // Arrange
        DeleteDirectory();
        Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), _directoryname));
        File.CreateText(Path.Combine(Path.GetTempPath(), _directoryname, "file1.txt"));
        File.CreateText(Path.Combine(Path.GetTempPath(), _directoryname, "file2.txt"));
        File.CreateText(Path.Combine(Path.GetTempPath(), _directoryname, "file3.txt"));
        Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), _directoryname, "dir"));
        Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), _directoryname, "dir2"));
        File.CreateText(Path.Combine(Path.GetTempPath(), _directoryname, "dir", "file1.txt"));
        File.CreateText(Path.Combine(Path.GetTempPath(), _directoryname, "dir2", "file2.txt"));

        // Act
        var directories = (await FS.GetDisk(_diskname).ListDirectoriesAsync(_directoryname)).ToArray();

        // Assert
        string[] names = directories.Select(directory => directory.Name).ToArray();
        Assert.Equal(2, directories.Length);
        Assert.Contains("dir", names);
        Assert.Contains("dir2", names);
    }

    public void Dispose()
    {
        DeleteDirectory();
        Container.Advanced.Clear();
    }

    private void DeleteDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), _directoryname);

        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }
}