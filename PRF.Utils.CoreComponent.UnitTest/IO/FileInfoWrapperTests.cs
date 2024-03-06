using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using PRF.Utils.CoreComponents.IO;

namespace PRF.Utils.CoreComponent.UnitTest.IO;

internal sealed class FileInfoWrapperTests
{
    private IFileInfo _sut;
    private string _testFilePath;
    private DirectoryInfoWrapper _testDirectory;

    [SetUp]
    public void TestInitialize()
    {
        // mock:
        _testDirectory = new DirectoryInfoWrapper(Path.Combine(TestContext.CurrentContext.TestDirectory, "Extensions"));
        _testDirectory.CleanDirectory();

        _sut = _testDirectory.GetFile("TestFile.txt");
        _sut.WriteAllText("Test content");
        _testFilePath = _sut.FullName;
    }

    [TearDown]
    public void TestCleanup()
    {
        _testDirectory.DeleteIfExistAndWaitDeletion(TimeSpan.FromSeconds(5));
    }

    [Test]
    public void DeleteIfExist_WhenFileExists_ShouldDeleteFile()
    {
        // Arrange

        // Act
        var result = _sut.DeleteIfExist();

        // Assert
        Assert.IsTrue(result);
        Assert.IsFalse(File.Exists(_testFilePath));
    }

    [Test]
    public void DeleteIfExist_WhenFileDoesNotExist_ShouldNotThrowException()
    {
        // Arrange
        File.Delete(_testFilePath); // Ensure file does not exist

        // Act & Assert
        Assert.DoesNotThrow(() => _sut.DeleteIfExist());
    }

    [Test]
    public void Directory_ShouldReturnParentDirectory()
    {
        // Arrange

        // Act
        var directory = _sut.Directory;

        // Assert
        Assert.IsNotNull(directory);
        Assert.AreEqual(Path.GetDirectoryName(_testFilePath), directory.FullName);
    }

    [Test]
    public void DirectoryName_ShouldReturnParentDirectoryPath()
    {
        // Arrange

        // Act
        var directoryName = _sut.DirectoryName;

        // Assert
        Assert.AreEqual(Path.GetDirectoryName(_testFilePath), directoryName);
    }

    [Test]
    public void DeleteIfExistAndWaitDeletion_WhenFileExists_ShouldDeleteFileAndWait()
    {
        // Arrange

        // Act
        var result = _sut.DeleteIfExistAndWaitDeletion();

        // Assert
        Assert.IsTrue(result);
        Assert.IsFalse(File.Exists(_testFilePath));
    }

    [Test]
    public void DeleteIfExistAndWaitDeletion_WhenFileDoesNotExist_ShouldNotThrowException()
    {
        // Arrange
        File.Delete(_testFilePath); // Ensure file does not exist

        // Act & Assert
        Assert.DoesNotThrow(() => _sut.DeleteIfExistAndWaitDeletion());
    }

    [Test]
    public void DeleteIfExistAndWaitDeletion_WhenTimeoutExceedsOneHour_ShouldThrowArgumentException()
    {
        // Arrange

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _sut.DeleteIfExistAndWaitDeletion(timeout: TimeSpan.FromHours(2)));
    }

    [Test]
    public async Task DeleteIfExistAndWaitDeletion_WhenFile_already_deleted()
    {
        // Arrange
        _sut.DeleteIfExist(); // Ensure file does not exist

        // Act
        var result = await Task.Run(() => _sut.DeleteIfExistAndWaitDeletion(timeout: TimeSpan.FromSeconds(1))).ConfigureAwait(false);

        // Assert
        Assert.IsTrue(result);
    }
}