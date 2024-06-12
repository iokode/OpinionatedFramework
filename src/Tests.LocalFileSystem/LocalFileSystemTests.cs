using System;
using System.IO;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.ContractImplementations.LocalFileSystem;
using IOKode.OpinionatedFramework.Facades;
using IOKode.OpinionatedFramework.FileSystem;
using IOKode.OpinionatedFramework.FileSystem.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using DirectoryNotFoundException = System.IO.DirectoryNotFoundException;
using File = System.IO.File;
using FileNotFoundException = IOKode.OpinionatedFramework.FileSystem.Exceptions.FileNotFoundException;

namespace IOKode.OpinionatedFramework.Tests.LocalFileSystem;

public class LocalFileSystemTests : IDisposable
{
    private const string _diskname = "temp";

    public LocalFileSystemTests()
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
    public async Task PutFileAndGetFile_Success()
    {
        // Arrange
        const string filename = "filename.txt";
        File.Delete(Path.Combine(Path.GetTempPath(), filename));

        // Assert (file not exists and throws exception when trying to get it)
        Assert.False(File.Exists(Path.Combine(Path.GetTempPath(), filename)));
        await Assert.ThrowsAsync<FileNotFoundException>(async () =>
        {
            await FS.GetDisk(_diskname).GetFileAsync(filename);
        });

        // Act (put file)
        {
            await using var file = new MemoryStream();
            await using var writer = new StreamWriter(file);
            await writer.WriteAsync("Text content.");
            await writer.FlushAsync();
            file.Position = 0;
            await FS.GetDisk(_diskname).PutFileAsync(filename, file);
        }

        // Assert (put file exists)
        Assert.True(File.Exists(Path.Combine(Path.GetTempPath(), filename)));

        {
            // Act (read file)
            await using var file = await (await FS.GetDisk(_diskname).GetFileAsync(filename)).OpenReadStreamAsync();
            using var reader = new StreamReader(file);
            string content = await reader.ReadToEndAsync();

            // Assert (read file)
            Assert.Equal("Text content.", content);
        }

        // Act & Assert (put file when exists)
        await Assert.ThrowsAsync<FileAlreadyExistsException>(async () =>
        {
            await FS.GetDisk(_diskname).PutFileAsync(filename, new MemoryStream());
        });
    }

    [Fact]
    public async Task FileExits_Exists()
    {
        // Arrange
        const string filename = "filename.txt";
        File.Create(Path.Combine(Path.GetTempPath(), filename));

        // Act
        bool exists = await FS.GetDisk(_diskname).ExistsFileAsync(filename);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task FileExits_NotExists()
    {
        // Arrange
        const string filename = "filename.txt";
        File.Delete(Path.Combine(Path.GetTempPath(), filename));

        // Act
        bool exists = await FS.GetDisk(_diskname).ExistsFileAsync(filename);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteFile_Success()
    {
        // Arrange
        const string filename = "filename.txt";
        File.Create(Path.Combine(Path.GetTempPath(), filename));

        // Act
        await FS.GetDisk(_diskname).DeleteFileAsync(filename);

        // Assert
        Assert.False(File.Exists(Path.Combine(Path.GetTempPath(), filename)));
    }

    [Fact]
    public async Task DeleteFile_NotExists()
    {
        // Arrange
        const string filename = "filename.txt";
        File.Delete(Path.Combine(Path.GetTempPath(), filename));

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(async () =>
        {
            await FS.GetDisk(_diskname).DeleteFileAsync(filename);
        });
    }

    [Fact]
    public async Task ReplaceFile_CreateFile()
    {
        // Arrange
        const string filename = "filename.txt";
        File.Delete(Path.Combine(Path.GetTempPath(), filename));

        // Act
        await FS.GetDisk(_diskname).ReplaceFileAsync(filename, new MemoryStream());

        // Assert
        Assert.True(File.Exists(Path.Combine(Path.GetTempPath(), filename)));
    }

    [Fact]
    public async Task ReplaceFile_ReplaceFile()
    {
        // Arrange
        const string filename = "filename.txt";
        {
            await using var firstFileStream = File.Create(Path.Combine(Path.GetTempPath(), filename));
            await firstFileStream.WriteAsync("Initial file content."u8.ToArray());
            await firstFileStream.FlushAsync();
        }

        // Assert (first file content)
        {
            var fileReader = File.OpenText(Path.Combine(Path.GetTempPath(), filename));
            string content = await fileReader.ReadToEndAsync();
            Assert.Equal("Initial file content.", content);
        }

        // Act (replace file)
        IOKode.OpinionatedFramework.FileSystem.File replacedFile;
        {
            using var file = new MemoryStream();
            await using var writer = new StreamWriter(file);
            await writer.WriteAsync("Second file content.");
            await writer.FlushAsync();
            file.Position = 0;
            replacedFile = await FS.GetDisk(_diskname).ReplaceFileAsync(filename, file);
        }

        // Assert (file is replaced)
        {
            using var fileReader = File.OpenText(Path.Combine(Path.GetTempPath(), filename));
            string content = await fileReader.ReadToEndAsync();

            Assert.Equal("Second file content.", content);
        }
        
        // Assert (check additional File properties)
        var fileInfo = new FileInfo(Path.Combine(Path.GetTempPath(), filename));
        Assert.Equal(filename, replacedFile.Name);
        Assert.Equal(fileInfo.FullName, replacedFile.FullName);
        Assert.Equal((ulong)20, replacedFile.Size);
        Assert.Equal(fileInfo.CreationTime, replacedFile.CreationTime);
    }

    [Fact]
    public async Task DirectoryShortcut()
    {
        // Arrange
        string path = Path.Combine(Path.GetTempPath(), "indir", "dir2", "file.txt");
        try
        {
            File.Delete(path);
        }
        catch (DirectoryNotFoundException)
        {
        }

        // Act
        await FS.GetDisk(_diskname).PutFileAsync("indir/dir2/file.txt", new MemoryStream("content"u8.ToArray()));

        // Assert
        Assert.True(File.Exists(path));
        Assert.Equal("content", await File.ReadAllTextAsync(path));
    }

    public void Dispose()
    {
        Container.Advanced.Clear();
    }
}