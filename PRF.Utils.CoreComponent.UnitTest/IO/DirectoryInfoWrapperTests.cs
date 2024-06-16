using System;
using System.IO;
using System.Linq;
using CommonUnitTest;
using PRF.Utils.CoreComponents.IO;

namespace PRF.Utils.CoreComponent.UnitTest.IO;

public sealed class DirectoryInfoWrapperTests : IDisposable
{
    private readonly IDirectoryInfo _sut;
    private readonly string _testDirectoryPath;
    private readonly DirectoryInfoWrapper _otherDir;

    public DirectoryInfoWrapperTests()
    {
        // mock:
        _sut = new DirectoryInfoWrapper(UnitTestFolder.Get("Dir"));
        _otherDir = new DirectoryInfoWrapper(UnitTestFolder.Get("OtherDir"));
        _testDirectoryPath = _sut.FullName;
        _sut.CleanDirectory();
    }

    public void Dispose()
    {
        _sut.DeleteIfExistAndWaitDeletion(TimeSpan.FromSeconds(5));
        _otherDir.DeleteIfExistAndWaitDeletion(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void EnumerateFiles_ShouldReturnFilesInDirectory()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testDirectoryPath, "File1.txt"), "Test content");
        File.WriteAllText(Path.Combine(_testDirectoryPath, "File2.txt"), "More content");

        // Act
        var files = _sut.EnumerateFiles().ToArray();

        // Assert
        Assert.Equal(2, files.Length);
        Assert.Contains(files, f => f.Name == "File1.txt");
        Assert.Contains(files, f => f.Name == "File2.txt");
    }

    [Fact]
    public void EnumerateFiles_WithSearchPattern_ShouldReturnMatchingFiles()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testDirectoryPath, "File1.txt"), "Test content");
        File.WriteAllText(Path.Combine(_testDirectoryPath, "File2.csv"), "CSV data");

        // Act
        var csvFiles = _sut.EnumerateFiles("*.csv").ToArray();

        // Assert
        Assert.Single(csvFiles);
        Assert.Equal("File2.csv", csvFiles[0].Name);
    }

    [Fact]
    public void EnumerateFiles_WithSearchOption_ShouldIncludeSubdirectories()
    {
        // Arrange
        Directory.CreateDirectory(Path.Combine(_testDirectoryPath, "Subdir"));
        File.WriteAllText(Path.Combine(_testDirectoryPath, "File1.txt"), "Test content");
        File.WriteAllText(Path.Combine(_testDirectoryPath, "Subdir", "File3.txt"), "Nested content");

        // Act
        var allFiles = _sut.EnumerateFiles("*", SearchOption.AllDirectories).ToArray();

        // Assert
        Assert.Equal(2, allFiles.Length);
        Assert.Contains(allFiles, f => f.Name == "File1.txt");
        Assert.Contains(allFiles, f => f.Name == "File3.txt");
    }

    [Fact]
    public void GetDirectories_ShouldReturnDirectoriesMatchingSearchPattern()
    {
        // Arrange
        Directory.CreateDirectory(Path.Combine(_testDirectoryPath, "Subdir1"));
        Directory.CreateDirectory(Path.Combine(_testDirectoryPath, "Subdir2"));
        Directory.CreateDirectory(Path.Combine(_testDirectoryPath, "OtherDir"));

        // Act
        var subdirs = _sut.GetDirectories("Subdir*");

        // Assert
        Assert.Equal(2, subdirs.Length);
        Assert.Contains(subdirs, d => d.Name == "Subdir1");
        Assert.Contains(subdirs, d => d.Name == "Subdir2");
    }

    [Fact]
    public void GetFiles_ShouldReturnAllFilesInDirectory()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testDirectoryPath, "File1.txt"), "Test content");
        File.WriteAllText(Path.Combine(_testDirectoryPath, "File2.csv"), "CSV data");

        // Act
        var files = _sut.GetFiles();

        // Assert
        Assert.Equal(2, files.Length);
        Assert.Contains(files, f => f.Name == "File1.txt");
        Assert.Contains(files, f => f.Name == "File2.csv");
    }

    [Fact]
    public void TryGetFile_WhenFileExists_ShouldReturnTrueAndSetFileInfo()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testDirectoryPath, "ExistingFile.txt"), "Test content");

        // Act
        var result = _sut.TryGetFile("ExistingFile.txt", out var fileInfo);

        // Assert
        Assert.True(result);
        Assert.NotNull(fileInfo);
        Assert.Equal("ExistingFile.txt", fileInfo.Name);
    }

    [Fact]
    public void MoveTo_ShouldMoveDirectoryToSpecifiedLocation()
    {
        // Arrange
        var destinationPath = Path.Combine(_otherDir.FullName, "NewLocation");
        _otherDir.CreateIfNotExists();

        // Act
        _sut.MoveTo(destinationPath);

        // Assert
        Assert.True(Directory.Exists(destinationPath));
        Assert.False(Directory.Exists(_testDirectoryPath));
    }

    [Fact]
    public void CopyTo_ShouldCreateTargetDirectoryAndCopyContents()
    {
        // Arrange
        var targetPath = _otherDir.GetDirectory("TargetDirectory").FullName;
        _sut.CreateSubdirectory("Subdir1").CreateFileIfNotExists("File1.txt");
        _sut.CreateSubdirectory("Subdir2").CreateFileIfNotExists("File2.txt");

        // Act
        var result = _sut.CopyTo(targetPath);

        // Assert
        Assert.NotNull(result);
        Assert.True(Directory.Exists(targetPath));
        Assert.True(Directory.Exists(Path.Combine(targetPath, "Subdir1")));
        Assert.True(Directory.Exists(Path.Combine(targetPath, "Subdir2")));
        Assert.True(File.Exists(Path.Combine(targetPath, "Subdir1", "File1.txt")));
        Assert.True(File.Exists(Path.Combine(targetPath, "Subdir2", "File2.txt")));
    }

    [Fact]
    public void CleanDirectory_ShouldRemoveAllFilesAndSubdirectories()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testDirectoryPath, "File1.txt"), "Test content");
        Directory.CreateDirectory(Path.Combine(_testDirectoryPath, "Subdir1"));
        File.WriteAllText(Path.Combine(_testDirectoryPath, "Subdir1", "File3.txt"), "Nested content");

        // Act
        var result = _sut.CleanDirectory();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(Directory.GetFiles(_testDirectoryPath));
        Assert.Empty(Directory.GetDirectories(_testDirectoryPath));
    }


    [Fact]
    public void EstimateSize_ShouldCalculateTotalSizeOfFiles()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testDirectoryPath, "File1.txt"), "Test content");
        File.WriteAllText(Path.Combine(_testDirectoryPath, "File2.csv"), "CSV data");

        // Act
        var totalSize = _sut.EstimateSize(SearchOption.AllDirectories);

        // Assert
        Assert.Equal(20, totalSize); // Assuming 10 bytes per file
    }

    [Fact]
    public void GetDirectory_ShouldReturnSubdirectoryIfExists()
    {
        // Arrange
        Directory.CreateDirectory(Path.Combine(_testDirectoryPath, "Subdir1"));

        // Act
        var subDir = _sut.GetDirectory("Subdir1");

        // Assert
        Assert.NotNull(subDir);
        Assert.Equal("Subdir1", subDir.Name);
    }

    [Fact]
    public void TryGetDirectory_WhenSubdirectoryExists_ShouldReturnTrueAndSetSubdirectory()
    {
        // Arrange
        Directory.CreateDirectory(Path.Combine(_testDirectoryPath, "ExistingSubdir"));

        // Act
        var result = _sut.TryGetDirectory("ExistingSubdir", out var subDir);

        // Assert
        Assert.True(result);
        Assert.NotNull(subDir);
        Assert.Equal("ExistingSubdir", subDir.Name);
    }
}