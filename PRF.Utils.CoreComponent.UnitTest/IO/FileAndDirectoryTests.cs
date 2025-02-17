using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CommonUnitTest;
using PRF.Utils.CoreComponents.Extensions;
using PRF.Utils.CoreComponents.IO;

namespace PRF.Utils.CoreComponent.UnitTest.IO;

public sealed class FileAndDirectoryTests : IDisposable
{
    private readonly IDirectoryInfo _testDirectory;

    public FileAndDirectoryTests()
    {
        // mock:
        _testDirectory = new DirectoryInfoWrapper(UnitTestFolder.Get("FileAndDirectoryTests"));
        _testDirectory.CleanDirectory();
    }

    public void Dispose()
    {
        _testDirectory.DeleteIfExistAndWaitDeletion(TimeSpan.FromSeconds(5));
    }


    [Fact]
    public void CreateDirectory_then_file_as_fluent()
    {
        //Arrange

        //Act
        var res = _testDirectory.CreateSubdirectoryIfNotExists("sub1").CreateSubdirectoryIfNotExists("sub2").CreateFileIfNotExists("foo.txt");

        //Assert
        Assert.NotNull(res);
        Assert.Equal(Path.Combine(_testDirectory.FullName, "sub1", "sub2", "foo.txt"), res.FullName);
    }

    [Fact]
    public void CreateDirectory_then_file_does_not_throw_if_duplicated()
    {
        //Arrange

        //Act
        _ = _testDirectory.CreateSubdirectoryIfNotExists("sub1").CreateSubdirectoryIfNotExists("sub2").CreateFileIfNotExists("foo.txt");
        var res = _testDirectory.CreateSubdirectoryIfNotExists("sub1").CreateSubdirectoryIfNotExists("sub2").CreateFileIfNotExists("foo.txt");

        //Assert
        Assert.NotNull(res);
        Assert.Equal(Path.Combine(_testDirectory.FullName, "sub1", "sub2", "foo.txt"), res.FullName);
    }

    [Fact]
    public void CreateDirectory_GetFile()
    {
        //Arrange

        //Act
        var res = _testDirectory.CreateSubdirectoryIfNotExists("sub1").GetFile("foo.txt");

        //Assert
        Assert.NotNull(res);
        Assert.False(res.Exists);
        Assert.False(res.ExistsExplicit);
        Assert.Equal(Path.Combine(_testDirectory.FullName, "sub1", "foo.txt"), res.FullName);
    }

    [Fact]
    public void CreateFileIfNotExist_create_the_file_even_if_directory_did_not_exists()
    {
        //Arrange
        _testDirectory.DeleteIfExistAndWaitDeletion(TimeSpan.FromSeconds(5));

        //Act
        var file = _testDirectory.CreateFileIfNotExists("foo.txt");

        //Assert
        Assert.True(file.Exists);
    }

    [Fact]
    public void ReadAllText_returns_file_content_as_string()
    {
        //Arrange
        var file = _testDirectory.CreateFileIfNotExists("testFile.txt");
        file.WriteAllText("Lorem ipsum");

        //Act
        var res = file.ReadAllText();

        //Assert
        Assert.Equal("Lorem ipsum", res);
    }

    [Fact]
    public void ReadAllLines_returns_file_content_as_string()
    {
        //Arrange
        var file = _testDirectory.CreateFileIfNotExists("testFile.txt");
        file.WriteAllText("Lorem ipsum");

        //Act
        var res = file.ReadAllLines().Single();

        //Assert
        Assert.Equal("Lorem ipsum", res);
    }

    [Fact]
    public void AppendAllText_write_file_content_as_string()
    {
        //Arrange
        var file = _testDirectory.CreateFileIfNotExists("testFile.txt");
        file.AppendAllText("Lorem ");
        file.AppendAllText("ipsum");

        //Act
        var res = file.ReadAllText();

        //Assert
        Assert.Equal("Lorem ipsum", res);
    }

    [Fact]
    public void GetDirectory_when_directory_does_not_exists()
    {
        //Arrange

        //Act
        var res = _testDirectory.GetDirectory("sub1");

        //Assert
        Assert.NotNull(res);
        Assert.False(res.Exists);
        Assert.Equal(Path.Combine(_testDirectory.FullName, "sub1"), res.FullName);
    }

    [Fact]
    public void Parent_is_valid()
    {
        //Arrange

        //Act
        var res = _testDirectory.CreateSubdirectoryIfNotExists("sub1").CreateSubdirectoryIfNotExists("sub2").CreateFileIfNotExists("foo.txt");

        //Assert
        Assert.NotNull(res);
        Assert.Equal(Path.Combine(_testDirectory.FullName, "sub1", "sub2"), res.Directory.FullName);
        Assert.Equal(Path.Combine(_testDirectory.FullName, "sub1"), res.Directory.Parent.FullName);
        Assert.Equal(_testDirectory.Root.FullName, res.Directory.Root.FullName);
    }

    [Fact]
    public void CreateSubdirectory_nominal()
    {
        //Arrange

        //Act
        var res = _testDirectory.CreateSubdirectory("sub1");

        //Assert
        Assert.NotNull(res);
        Assert.Equal(Path.Combine(_testDirectory.FullName, "sub1"), res.FullName);
    }

    [Fact]
    public void GetDirectories_nominal()
    {
        //Arrange
        _testDirectory.CreateSubdirectory("sub1");
        _testDirectory.CreateSubdirectory("sub2");

        //Act
        var subDir = _testDirectory.GetDirectories();

        //Assert
        Assert.Equal(2, subDir.Length);
    }

    [Fact]
    public void GetFiles_nominal()
    {
        //Arrange
        _testDirectory.CreateSubdirectory("sub1");
        _testDirectory.CreateSubdirectory("sub2");
        _testDirectory.CreateFileIfNotExists("foo.txt");

        //Act
        var files = _testDirectory.GetFiles();

        //Assert
        Assert.Single(files);
    }

    [Fact]
    public void GetFiles_nominal_SearchOption_AllDirectories()
    {
        //Arrange
        _testDirectory.CreateSubdirectory("sub1");
        var subDir = _testDirectory.CreateSubdirectory("sub2");
        subDir.CreateFileIfNotExists("foo.txt");

        //Act
        var files = _testDirectory.GetFiles("*.*", SearchOption.AllDirectories);

        //Assert
        Assert.Single(files);
    }

    [Fact]
    public void IsEmpty_true()
    {
        //Arrange

        //Act

        //Assert
        Assert.True(_testDirectory.IsEmpty());
    }

    [Fact]
    public void IsEmpty_false()
    {
        //Arrange
        _testDirectory.CreateSubdirectory("sub1");

        //Act

        //Assert
        Assert.False(_testDirectory.IsEmpty());
    }

    [Fact]
    public void IsEmpty_false_file()
    {
        //Arrange
        _testDirectory.CreateFileIfNotExists("foo.txt");

        //Act

        //Assert
        Assert.False(_testDirectory.IsEmpty());
    }


    [Fact]
    public void EnumerateDirectories_nominal()
    {
        //Arrange
        _testDirectory.CreateSubdirectory("sub1");
        _testDirectory.CreateSubdirectory("sub2");

        //Act
        var subDir = _testDirectory.EnumerateDirectories();

        //Assert
        Assert.Equal(2, subDir.Count());
    }

    [Fact]
    public void EnumerateFiles_nominal()
    {
        //Arrange
        _testDirectory.CreateSubdirectory("sub1");
        _testDirectory.CreateSubdirectory("sub2");
        _testDirectory.CreateFileIfNotExists("foo.txt");

        //Act
        var files = _testDirectory.EnumerateFiles();

        //Assert
        Assert.Single(files);
    }

    [Fact]
    public void EnumerateFiles_nominal_SearchOption_AllDirectories()
    {
        //Arrange
        _testDirectory.CreateSubdirectory("sub1");
        var subDir = _testDirectory.CreateSubdirectory("sub2");
        subDir.CreateFileIfNotExists("foo.txt");

        //Act
        var files = _testDirectory.EnumerateFiles("*.*", SearchOption.AllDirectories);

        //Assert
        Assert.Single(files);
    }

    [Fact]
    public void DeleteIfExist_when_does_not_exists()
    {
        //Arrange
        var file = _testDirectory.GetFile("foo.txt");

        //Act
        var res = file.DeleteIfExist();

        //Assert
        Assert.False(res);
        Assert.False(file.ExistsExplicit);
    }

    [Fact]
    public void DeleteIfExist_when_does_exists()
    {
        //Arrange
        var file = _testDirectory.CreateFileIfNotExists("foo.txt");

        //Act
        var res = file.DeleteIfExist();

        //Assert
        Assert.True(res);
        Assert.False(file.ExistsExplicit);
    }

    [Fact]
    public void File_State_tests()
    {
        //Arrange

        //Act
        var file = _testDirectory.CreateFileIfNotExists("foo.txt");

        //Assert
        Assert.Equal(_testDirectory.FullName, file.DirectoryName);
        Assert.False(file.IsReadOnly);
        Assert.Equal(0, file.Length);
    }

    [Fact]
    public void TryDelete_ShouldDeleteFileIfNotInUse()
    {
        // Arrange
        var testFilePath = _testDirectory.GetFile("foo.txt").FullName;
        var fileInfo = new FileInfo(testFilePath);

        // Act
        var success = fileInfo.TryDelete();

        // Assert
        Assert.True(success);
        Assert.False(File.Exists(testFilePath));
    }

    [Fact]
    public void TryDelete_WithTimeout_ShouldDeleteFileWithinTimeout()
    {
        // Arrange
        var testFilePath = _testDirectory.GetFile("foo.txt").FullName;
        var fileInfo = new FileInfo(testFilePath);
        var timeout = TimeSpan.FromSeconds(5);

        // Act
        var success = fileInfo.TryDelete(timeout);

        // Assert
        Assert.True(success);
        Assert.False(File.Exists(testFilePath));
    }

    [Fact]
    public void TryDelete_WithInvalidTimeout_ShouldThrowArgumentException()
    {
        // Arrange
        var testFilePath = _testDirectory.GetFile("foo.txt").FullName;
        var fileInfo = new FileInfo(testFilePath);
        var invalidTimeout = TimeSpan.FromHours(2);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => fileInfo.TryDelete(invalidTimeout));
    }

    [Fact]
    public void DeleteAndRetryIfLocked_ShouldDeleteFileWithRetries()
    {
        // Arrange
        var testFilePath = _testDirectory.CreateFileIfNotExists("foo.txt").FullName;
        var fileInfo = new FileInfo(testFilePath);

        // Act
        fileInfo.DeleteAndRetryIfLocked();

        // Assert
        Assert.False(File.Exists(testFilePath));
    }

    [Fact]
    public void DeleteAndRetryIfLocked_returns_if_already_deleted()
    {
        // Arrange
        var testFilePath = _testDirectory.GetFile("foo.txt").FullName;
        File.Delete(testFilePath);
        var fileInfo = new FileInfo(testFilePath);

        // Act
        fileInfo.DeleteAndRetryIfLocked();

        // Assert
        Assert.False(File.Exists(testFilePath));
    }

    [Fact]
    public async Task DeleteAndRetryIfLocked_WhenFileIsLocked_ShouldThrowTimeoutException()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // delete while using is only enforced on windows
            return;
        }
        // Arrange
        var testFilePath = _testDirectory.CreateFileIfNotExists("foo.txt").FullName;

        // do not allow deletion
        await using (var _ = File.Open(testFilePath, FileMode.Open, FileAccess.Read, FileShare.None))
        {
            var fileInfo = new FileInfo(testFilePath);

            // Act & Assert
            Assert.Throws<TimeoutException>(() => fileInfo.DeleteAndRetryIfLocked(TimeSpan.FromMilliseconds(300)));
        }
    }
}