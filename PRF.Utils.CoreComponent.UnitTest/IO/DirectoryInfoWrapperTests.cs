using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PRF.Utils.CoreComponents.IO;

namespace PRF.Utils.CoreComponent.UnitTest.IO;

internal sealed class DirectoryInfoWrapperTests
{
    private IDirectoryInfo _sut;
    private string _testDirectoryPath;
    private DirectoryInfoWrapper _otherDir;

    [SetUp]
    public void TestInitialize()
    {
        // mock:
        _sut = new DirectoryInfoWrapper(Path.Combine(TestContext.CurrentContext.TestDirectory, "Dir"));
        _otherDir = new DirectoryInfoWrapper(Path.Combine(TestContext.CurrentContext.TestDirectory, "OtherDir"));
        _testDirectoryPath = _sut.FullName;
        _sut.CleanDirectory();
    }

    [TearDown]
    public void TestCleanup()
    {
        _sut.DeleteIfExistAndWaitDeletion(TimeSpan.FromSeconds(5));
        _otherDir.DeleteIfExistAndWaitDeletion(TimeSpan.FromSeconds(5));
    }

    [Test]
    public void EnumerateFiles_ShouldReturnFilesInDirectory()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testDirectoryPath, "File1.txt"), "Test content");
        File.WriteAllText(Path.Combine(_testDirectoryPath, "File2.txt"), "More content");

        // Act
        var files = _sut.EnumerateFiles().ToArray();

        // Assert
        Assert.AreEqual(2, files.Length);
        Assert.IsTrue(files.Any(f => f.Name == "File1.txt"));
        Assert.IsTrue(files.Any(f => f.Name == "File2.txt"));
    }

    [Test]
    public void EnumerateFiles_WithSearchPattern_ShouldReturnMatchingFiles()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testDirectoryPath, "File1.txt"), "Test content");
        File.WriteAllText(Path.Combine(_testDirectoryPath, "File2.csv"), "CSV data");

        // Act
        var csvFiles = _sut.EnumerateFiles("*.csv").ToArray();

        // Assert
        Assert.AreEqual(1, csvFiles.Length);
        Assert.AreEqual("File2.csv", csvFiles[0].Name);
    }

    [Test]
    public void EnumerateFiles_WithSearchOption_ShouldIncludeSubdirectories()
    {
        // Arrange
        Directory.CreateDirectory(Path.Combine(_testDirectoryPath, "Subdir"));
        File.WriteAllText(Path.Combine(_testDirectoryPath, "File1.txt"), "Test content");
        File.WriteAllText(Path.Combine(_testDirectoryPath, "Subdir", "File3.txt"), "Nested content");

        // Act
        var allFiles = _sut.EnumerateFiles("*", SearchOption.AllDirectories).ToArray();

        // Assert
        Assert.AreEqual(2, allFiles.Length);
        Assert.IsTrue(allFiles.Any(f => f.Name == "File1.txt"));
        Assert.IsTrue(allFiles.Any(f => f.Name == "File3.txt"));
    }

    [Test]
    public void GetDirectories_ShouldReturnDirectoriesMatchingSearchPattern()
    {
        // Arrange
        Directory.CreateDirectory(Path.Combine(_testDirectoryPath, "Subdir1"));
        Directory.CreateDirectory(Path.Combine(_testDirectoryPath, "Subdir2"));
        Directory.CreateDirectory(Path.Combine(_testDirectoryPath, "OtherDir"));

        // Act
        var subdirs = _sut.GetDirectories("Subdir*");

        // Assert
        Assert.AreEqual(2, subdirs.Length);
        Assert.IsTrue(subdirs.Any(d => d.Name == "Subdir1"));
        Assert.IsTrue(subdirs.Any(d => d.Name == "Subdir2"));
    }

    [Test]
    public void GetFiles_ShouldReturnAllFilesInDirectory()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testDirectoryPath, "File1.txt"), "Test content");
        File.WriteAllText(Path.Combine(_testDirectoryPath, "File2.csv"), "CSV data");

        // Act
        var files = _sut.GetFiles();

        // Assert
        Assert.AreEqual(2, files.Length);
        Assert.IsTrue(files.Any(f => f.Name == "File1.txt"));
        Assert.IsTrue(files.Any(f => f.Name == "File2.csv"));
    }

    [Test]
    public void TryGetFile_WhenFileExists_ShouldReturnTrueAndSetFileInfo()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testDirectoryPath, "ExistingFile.txt"), "Test content");

        // Act
        var result = _sut.TryGetFile("ExistingFile.txt", out var fileInfo);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNotNull(fileInfo);
        Assert.AreEqual("ExistingFile.txt", fileInfo.Name);
    }

    [Test]
    public void MoveTo_ShouldMoveDirectoryToSpecifiedLocation()
    {
        // Arrange
        var destinationPath = Path.Combine(_otherDir.FullName, "NewLocation");
        _otherDir.CreateIfNotExists();

        // Act
        _sut.MoveTo(destinationPath);

        // Assert
        Assert.IsTrue(Directory.Exists(destinationPath));
        Assert.IsFalse(Directory.Exists(_testDirectoryPath));
    }

    [Test]
    public void CopyTo_ShouldCreateTargetDirectoryAndCopyContents()
    {
        // Arrange
        var targetPath = _otherDir.GetDirectory("TargetDirectory").FullName;
        _sut.CreateSubdirectory("Subdir1").CreateFileIfNotExists("File1.txt");
        _sut.CreateSubdirectory("Subdir2").CreateFileIfNotExists("File2.txt");

        // Act
        var result = _sut.CopyTo(targetPath);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(Directory.Exists(targetPath));
        Assert.IsTrue(Directory.Exists(Path.Combine(targetPath, "Subdir1")));
        Assert.IsTrue(Directory.Exists(Path.Combine(targetPath, "Subdir2")));
        Assert.IsTrue(File.Exists(Path.Combine(targetPath, "Subdir1", "File1.txt")));
        Assert.IsTrue(File.Exists(Path.Combine(targetPath, "Subdir2", "File2.txt")));
    }

    [Test]
    public void CleanDirectory_ShouldRemoveAllFilesAndSubdirectories()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testDirectoryPath, "File1.txt"), "Test content");
        Directory.CreateDirectory(Path.Combine(_testDirectoryPath, "Subdir1"));
        File.WriteAllText(Path.Combine(_testDirectoryPath, "Subdir1", "File3.txt"), "Nested content");

        // Act
        var result = _sut.CleanDirectory();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, Directory.GetFiles(_testDirectoryPath).Length);
        Assert.AreEqual(0, Directory.GetDirectories(_testDirectoryPath).Length);
    }


    [Test]
    public void EstimateSize_ShouldCalculateTotalSizeOfFiles()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testDirectoryPath, "File1.txt"), "Test content");
        File.WriteAllText(Path.Combine(_testDirectoryPath, "File2.csv"), "CSV data");

        // Act
        var totalSize = _sut.EstimateSize(SearchOption.AllDirectories);

        // Assert
        Assert.AreEqual(20, totalSize); // Assuming 10 bytes per file
    }

    [Test]
    public void GetDirectory_ShouldReturnSubdirectoryIfExists()
    {
        // Arrange
        Directory.CreateDirectory(Path.Combine(_testDirectoryPath, "Subdir1"));

        // Act
        var subDir = _sut.GetDirectory("Subdir1");

        // Assert
        Assert.IsNotNull(subDir);
        Assert.AreEqual("Subdir1", subDir.Name);
    }

    [Test]
    public void TryGetDirectory_WhenSubdirectoryExists_ShouldReturnTrueAndSetSubdirectory()
    {
        // Arrange
        Directory.CreateDirectory(Path.Combine(_testDirectoryPath, "ExistingSubdir"));

        // Act
        var result = _sut.TryGetDirectory("ExistingSubdir", out var subDir);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNotNull(subDir);
        Assert.AreEqual("ExistingSubdir", subDir.Name);
    }
}