using System;
using System.IO;
using System.Threading.Tasks;
using CommonUnitTest;
using PRF.Utils.CoreComponents.IO;

namespace PRF.Utils.CoreComponent.UnitTest.IO;

public sealed class FileInfoWrapperTests : IDisposable
{
    private readonly IFileInfo _sut;
    private readonly string _testFilePath;
    private readonly DirectoryInfoWrapper _testDirectory;

    public FileInfoWrapperTests()
    {
        // mock:
        _testDirectory = new DirectoryInfoWrapper(UnitTestFolder.Get("FileInfoWrapperTests"));
        _testDirectory.CleanDirectory();

        _sut = _testDirectory.GetFile("TestFile.txt");
        _sut.WriteAllText("Test content");
        _testFilePath = _sut.FullName;
    }

    public void Dispose()
    {
        _testDirectory.DeleteIfExistAndWaitDeletion(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void DeleteIfExist_WhenFileExists_ShouldDeleteFile()
    {
        // Arrange

        // Act
        var result = _sut.DeleteIfExist();

        // Assert
        Assert.True(result);
        Assert.False(File.Exists(_testFilePath));
    }

    [Fact]
    public void DeleteIfExist_WhenFileDoesNotExist_ShouldNotThrowException()
    {
        // Arrange
        File.Delete(_testFilePath); // Ensure file does not exist

        // Act & Assert (does not throw)
        _sut.DeleteIfExist();
    }

    [Fact]
    public void Directory_ShouldReturnParentDirectory()
    {
        // Arrange

        // Act
        var directory = _sut.Directory;

        // Assert
        Assert.NotNull(directory);
        Assert.Equal(Path.GetDirectoryName(_testFilePath), directory.FullName);
    }

    [Fact]
    public void DirectoryName_ShouldReturnParentDirectoryPath()
    {
        // Arrange

        // Act
        var directoryName = _sut.DirectoryName;

        // Assert
        Assert.Equal(Path.GetDirectoryName(_testFilePath), directoryName);
    }

    [Fact]
    public void DeleteIfExistAndWaitDeletion_WhenFileExists_ShouldDeleteFileAndWait()
    {
        // Arrange

        // Act
        var result = _sut.DeleteIfExistAndWaitDeletion();

        // Assert
        Assert.True(result);
        Assert.False(File.Exists(_testFilePath));
    }

    [Fact]
    public void DeleteIfExistAndWaitDeletion_WhenFileDoesNotExist_ShouldNotThrowException()
    {
        // Arrange
        File.Delete(_testFilePath); // Ensure file does not exist

        // Act & Assert (does not throw)
        _sut.DeleteIfExistAndWaitDeletion();
    }

    [Fact]
    public void DeleteIfExistAndWaitDeletion_WhenTimeoutExceedsOneHour_ShouldThrowArgumentException()
    {
        // Arrange

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _sut.DeleteIfExistAndWaitDeletion(timeout: TimeSpan.FromHours(2)));
    }

    [Fact]
    public async Task DeleteIfExistAndWaitDeletion_WhenFile_already_deleted()
    {
        // Arrange
        _sut.DeleteIfExist(); // Ensure file does not exist

        // Act
        var result = await Task.Run(() => _sut.DeleteIfExistAndWaitDeletion(timeout: TimeSpan.FromSeconds(1))).ConfigureAwait(true);

        // Assert
        Assert.True(result);
    }
}