using System.Text;
using IOKode.OpinionatedFramework.ConfigureApplication;
using IOKode.OpinionatedFramework.ContractImplementations.LocalFileSystem;
using IOKode.OpinionatedFramework.Contracts;
using IOKode.OpinionatedFramework.Facades;
using IOKode.OpinionatedFramework.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FileNotFoundException = IOKode.OpinionatedFramework.FileSystem.FileNotFoundException;

namespace Tests.LocalFileSystem;

public class LocalFileSystemTests : IDisposable
{
    private const string _diskname = "temp";

    public LocalFileSystemTests()
    {
        Container.Services.AddSingleton<IFileSystem>(_ =>
        {
            var fileSystem = new FileSystem();
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

        // Initial assert
        Assert.False(await FS.GetDisk(_diskname).ExistsFileAsync(filename));
        await Assert.ThrowsAsync<FileNotFoundException>(async () =>
        {
            await FS.GetDisk(_diskname).GetFileAsync(filename);
        });

        // Act
        {
            await using var file = new MemoryStream();
            await using var writer = new StreamWriter(file);
            await writer.WriteAsync("Text content.");
            await writer.FlushAsync();
            file.Position = 0;
            await FS.GetDisk(_diskname).PutFileAsync(filename, file);
        }

        // Assert
        {
            Assert.True(await FS.GetDisk(_diskname).ExistsFileAsync(filename));
            await using var file = await FS.GetDisk(_diskname).GetFileAsync(filename);
            using var reader = new StreamReader(file);
            string content = await reader.ReadToEndAsync();
            Assert.Equal("Text content.", content);
            await Assert.ThrowsAsync<FileAlreadyExistsException>(async () =>
            {
                await FS.GetDisk(_diskname).PutFileAsync(filename, new MemoryStream());
            });
        }
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
            var file = File.OpenText(Path.Combine(Path.GetTempPath(), filename));
            string fileContent = await file.ReadToEndAsync();
            Assert.Equal("Initial file content.", fileContent);
        }

        // Act (replace file)
        {
            using var secondFileStream = new MemoryStream();
            await using var writer = new StreamWriter(secondFileStream);
            await writer.WriteAsync("Second file content.");
            await writer.FlushAsync();
            secondFileStream.Position = 0;
            await FS.GetDisk(_diskname).ReplaceFileAsync(filename, secondFileStream);
        }

        // Assert (file is replaced)
        {
            using var file = File.OpenText(Path.Combine(Path.GetTempPath(), filename));
            string content = await file.ReadToEndAsync();

            Assert.Equal("Second file content.", content);
        }
    }

    public void Dispose()
    {
        Container.Clear();
    }
}