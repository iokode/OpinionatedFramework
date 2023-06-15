using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.ConfigureApplication;
using IOKode.OpinionatedFramework.ContractImplementations.FileSystem;
using IOKode.OpinionatedFramework.ContractImplementations.LocalFileSystem;
using IOKode.OpinionatedFramework.Contracts;
using IOKode.OpinionatedFramework.Facades;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests.LocalFileSystem;

public class LocalDirectoryTests : IDisposable
{
    private const string _diskname = "temp";
    private const string _directoryname = "dirname";

    public LocalDirectoryTests()
    {
        Container.Clear();
        Container.Services.AddSingleton<IFileSystem>(_ =>
        {
            var fileSystem = new FileSystem();
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
        
        // Act - List files in root
        string[] itemsInRoot = (await FS.GetDisk(_diskname).ListFilesAsync(_directoryname)).ToArray();
        
        // Assert three files in root
        Assert.Equal(3, itemsInRoot.Length);
        Assert.Contains("file1.txt", itemsInRoot);
        Assert.Contains("file2.txt", itemsInRoot);
        Assert.Contains("file3.txt", itemsInRoot);
        
        // Act - List files in directory
        string[] itemsInDirectory = (await FS.GetDisk(_diskname).ListFilesAsync(Path.Combine(_directoryname, "dir"))).ToArray();
        
        // Assert two files in root
        Assert.Equal(2, itemsInDirectory.Length);
        Assert.Contains("file1.txt", itemsInDirectory);
        Assert.Contains("file2.txt", itemsInDirectory);
    }

    public void Dispose()
    {
        DeleteDirectory();
        Container.Clear();
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