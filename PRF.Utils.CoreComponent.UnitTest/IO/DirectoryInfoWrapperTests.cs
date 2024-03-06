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

    [SetUp]
    public void TestInitialize()
    {
        // mock:
        _sut = new DirectoryInfoWrapper(Path.Combine(TestContext.CurrentContext.TestDirectory, "Extensions"));
        _testDirectoryPath = _sut.FullName;
        _sut.CleanDirectory();
    }

    [TearDown]
    public void TestCleanup()
    {
        _sut.DeleteIfExistAndWaitDeletion(TimeSpan.FromSeconds(5));
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
}