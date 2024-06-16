using System;
using System.IO;
using CommonUnitTest;
using PRF.Utils.CoreComponents.Extensions;

// ReSharper disable StringLiteralTypo

namespace PRF.Utils.CoreComponent.UnitTest.Extensions;

public sealed class FileAndDirectoryExtensionsTests : IDisposable
{
    private readonly DirectoryInfo _testDirectory;

    public FileAndDirectoryExtensionsTests()
    {
        // mock:
        _testDirectory = UnitTestFolder.Get("FileAndDirectoryExtensionsTests");
        _testDirectory.CreateIfNotExist();
    }

    public void Dispose()
    {
        _testDirectory.DeleteIfExistAndWaitDeletion(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GetFile_returns_null_for_null_input()
    {
        //Arrange

        //Act
        var res = _testDirectory.GetFile(null);

        //Assert
        Assert.Null(res);
    }

    [Fact]
    public void GetFile_returns_file_for_existing_file()
    {
        //Arrange
        _testDirectory.CreateFileIfNotExist("testFile.txt");

        //Act
        var res = _testDirectory.GetFile("testFile.txt");

        //Assert
        Assert.NotNull(res);
        Assert.True(res.Exists);
    }


    [Fact]
    public void GetFile_returns_file_for_non_existing_file()
    {
        //Arrange

        //Act
        var res = _testDirectory.GetFile("unknown.txt");

        //Assert
        Assert.NotNull(res);
        Assert.False(res.Exists);
    }

    [Fact]
    public void GetRelativePath_returns_correct_relative_path()
    {
        //Arrange
        var file = new FileInfo(Path.Combine(_testDirectory.FullName, "testFile.txt"));

        //Act
        // ReSharper disable once PossibleNullReferenceException
        var res = file.GetRelativePath(_testDirectory.Parent.FullName);

        //Assert
        Assert.Equal($"FileAndDirectoryExtensionsTests{Path.DirectorySeparatorChar}testFile.txt", res);
    }

    [Fact]
    public void IsEmpty_returns_true_when_directory_is_empty()
    {
        //Arrange
        var tempDirectory = _testDirectory.CreateSubdirectoryIfNotExist(Guid.NewGuid().ToString());

        try
        {
            //Act
            var res = tempDirectory.IsEmpty();

            //Assert
            Assert.True(res);
        }
        finally
        {
            tempDirectory.DeleteIfExistAndWaitDeletion();
        }
    }

    [Fact]
    public void IsEmpty_returns_false_when_directory_not_empty()
    {
        //Arrange
        _testDirectory.CreateFileIfNotExist("foo.txt");

        //Act
        var res = _testDirectory.IsEmpty();

        //Assert
        Assert.False(res);
    }

    [Fact]
    public void CreateFileIfNotExist_without_rename()
    {
        //Arrange

        //Act
        var file = _testDirectory.CreateFileIfNotExist("foo.txt");

        try
        {
            //Assert
            Assert.True(file.Exists);
        }
        finally
        {
            file.DeleteIfExistAndWaitDeletion();
        }
    }

    [Fact]
    public void EstimateSize_return_correct_estimation()
    {
        //Arrange

        //Act
        var size = _testDirectory.EstimateSize(SearchOption.AllDirectories);

        //Assert
        Assert.Equal(0, size);
    }

    [Fact]
    public void IsOldEnough_return_false_for_very_big_timespan()
    {
        //Arrange
        var file = _testDirectory.GetFile("testFile.txt");

        //Act
        var res = file.IsOldEnough(TimeSpan.MaxValue);

        //Assert
        Assert.False(res);
    }

    [Fact]
    public void IsOldEnough_return_false_for_zero_timespan()
    {
        //Arrange
        var file = _testDirectory.GetFile("testFile.txt");

        //Act
        var res = file.IsOldEnough(TimeSpan.Zero);

        //Assert
        Assert.True(res);
    }

    [Fact]
    public void CreateDirectoryAndTryClean_create_the_directory_if_it_does_not_exists()
    {
        //Arrange
        var newDirectory = _testDirectory.Parent.GetDirectory(Guid.NewGuid().ToString());

        try
        {
            //Act
            var res = newDirectory.CreateDirectoryAndTryClean();

            //Assert
            Assert.True(res.Exists);
        }
        finally
        {
            newDirectory.DeleteIfExistAndWaitDeletion();
        }
    }

    [Fact]
    public void CreateDirectoryAndTryClean_clean_the_directory()
    {
        //Arrange
        var newDirectory = _testDirectory.Parent.CreateSubdirectoryIfNotExist(Guid.NewGuid().ToString());
        // create files and sub-directory:
        newDirectory.CreateFileIfNotExist("foo.txt");
        newDirectory.CreateSubdirectoryIfNotExist("subDir").CreateFileIfNotExist("subfileFoo.txt");

        try
        {
            //Act
            var res = newDirectory.CreateDirectoryAndTryClean();

            //Assert
            Assert.True(res.Exists);
            Assert.True(res.IsEmpty());
        }
        finally
        {
            newDirectory.DeleteIfExistAndWaitDeletion();
        }
    }

    [Fact]
    public void CleanDirectory_create_the_directory_if_it_does_not_exists()
    {
        //Arrange
        var newDirectory = _testDirectory.Parent.GetDirectory(Guid.NewGuid().ToString());
        try
        {
            //Act
            var res = newDirectory.CleanDirectory();

            //Assert
            Assert.True(res.Exists);
        }
        finally
        {
            newDirectory.DeleteIfExistAndWaitDeletion();
        }
    }

    [Fact]
    public void CleanDirectory_clean_the_directory_when_it_exists()
    {
        //Arrange
        var newDirectory = _testDirectory.Parent.CreateSubdirectoryIfNotExist(Guid.NewGuid().ToString());
        // create files and sub-directory:
        newDirectory.CreateFileIfNotExist("foo.txt");
        newDirectory.CreateSubdirectoryIfNotExist("subDir").CreateFileIfNotExist("subfileFoo.txt");

        try
        {
            //Act
            var res = newDirectory.CleanDirectory();

            //Assert
            Assert.True(res.Exists);
            Assert.True(res.IsEmpty());
        }
        finally
        {
            newDirectory.DeleteIfExistAndWaitDeletion();
        }
    }

    [Fact]
    public void CreateIfNotExistAndClose_returns_true_when_the_file_is_created()
    {
        //Arrange
        var newDirectory = _testDirectory.Parent.CreateSubdirectoryIfNotExist(Guid.NewGuid().ToString());
        var file = newDirectory.GetFile("foo.txt");

        try
        {
            //Act
            var res = file.CreateIfNotExistAndClose();

            //Assert
            Assert.True(res);
            Assert.True(file.Exists);
        }
        finally
        {
            newDirectory.DeleteIfExistAndWaitDeletion();
        }
    }

    [Fact]
    public void CreateIfNotExistAndClose_returns_false_when_the_file_is_already_created()
    {
        //Arrange
        var newDirectory = _testDirectory.Parent.CreateSubdirectoryIfNotExist(Guid.NewGuid().ToString());
        var file = newDirectory.CreateFileIfNotExist("foo.txt");

        try
        {
            //Act
            var res = file.CreateIfNotExistAndClose();

            //Assert
            Assert.False(res);
            Assert.True(file.Exists);
        }
        finally
        {
            newDirectory.DeleteIfExistAndWaitDeletion();
        }
    }

    [Fact]
    public void Directory_CreateIfNotExist_returns_false_when_the_directory_is_already_created()
    {
        //Arrange
        var newDirectory = _testDirectory.Parent.CreateSubdirectoryIfNotExist(Guid.NewGuid().ToString());

        try
        {
            //Act
            var res = newDirectory.CreateIfNotExist();

            //Assert
            Assert.False(res);
            Assert.True(newDirectory.Exists);
        }
        finally
        {
            newDirectory.DeleteIfExistAndWaitDeletion();
        }
    }

    [Fact]
    public void Directory_CreateIfNotExist_returns_true_when_the_directory_is_created()
    {
        //Arrange
        var newDirectory = _testDirectory.GetDirectory(Guid.NewGuid().ToString());

        try
        {
            //Act
            var res = newDirectory.CreateIfNotExist();

            //Assert
            Assert.True(res);
            Assert.True(newDirectory.Exists);
        }
        finally
        {
            newDirectory.DeleteIfExistAndWaitDeletion();
        }
    }

    [Fact]
    public void DeleteIfExist_returns_true_when_the_file_is_deleted()
    {
        //Arrange
        var newDirectory = _testDirectory.CreateSubdirectoryIfNotExist(Guid.NewGuid().ToString());
        var file = newDirectory.CreateFileIfNotExist("foo.txt");

        try
        {
            //Act
            var res = file.DeleteIfExist();

            //Assert
            Assert.True(res);
            Assert.False(file.Exists);
        }
        finally
        {
            newDirectory.DeleteIfExistAndWaitDeletion();
        }
    }

    [Fact]
    public void DeleteIfExist_returns_false_when_the_file_is_already_deleted()
    {
        //Arrange
        var newDirectory = _testDirectory.CreateSubdirectoryIfNotExist(Guid.NewGuid().ToString());
        var file = newDirectory.GetFile("foo.txt");

        try
        {
            //Act
            var res = file.DeleteIfExist();

            //Assert
            Assert.False(res);
            Assert.False(file.Exists);
        }
        finally
        {
            newDirectory.DeleteIfExistAndWaitDeletion();
        }
    }

    [Fact]
    public void AutoRenameFileToAvoidDuplicate_returns_available_name()
    {
        //Arrange
        var newDirectory = _testDirectory.CreateSubdirectoryIfNotExist(Guid.NewGuid().ToString());
        var file = newDirectory.CreateFileIfNotExist("foo.txt");

        try
        {
            //Act
            var res = new FileInfo(FilesAndDirectoryInfoExtension.AutoRenameFileToAvoidDuplicate(file.FullName));

            //Assert
            Assert.Equal(newDirectory.FullName, res.Directory?.FullName);
            Assert.Equal("foo(2).txt", res.Name);
        }
        finally
        {
            newDirectory.DeleteIfExistAndWaitDeletion();
        }
    }

    [Fact]
    public void AutoRenameFileToAvoidDuplicate_returns_available_name_with_more_file()
    {
        //Arrange
        var newDirectory = _testDirectory.CreateSubdirectoryIfNotExist(Guid.NewGuid().ToString());
        var file = newDirectory.CreateFileIfNotExist("foo.txt");

        try
        {
            for (var i = 2; i < 10; i++)
            {
                //Act
                var res = new FileInfo(FilesAndDirectoryInfoExtension.AutoRenameFileToAvoidDuplicate(file.FullName));
                res.CreateIfNotExistAndClose(); // create it for the next iteration

                //Assert
                Assert.Equal(newDirectory.FullName, res.Directory?.FullName);
                Assert.Equal($"foo({i}).txt", res.Name);
            }
        }
        finally
        {
            newDirectory.DeleteIfExistAndWaitDeletion();
        }
    }

    [Fact]
    public void ContainsInvalidCharFromName_returns_true_if_file_contains_invalid_char()
    {
        //Arrange

        //Act
        var res = FilesAndDirectoryInfoExtension.ContainsInvalidCharFromName("dfds//fù##?<>sdf");

        //Assert
        Assert.True(res);
    }

    [Fact]
    public void ContainsInvalidCharFromName_returns_true_if_file_contains_invalid_char_slash()
    {
        //Arrange

        //Act
        var res = FilesAndDirectoryInfoExtension.ContainsInvalidCharFromName("dfdsf/sdf");

        //Assert
        Assert.True(res);
    }

    [Fact]
    public void ContainsInvalidCharFromName_returns_false_if_file_does_not_contains_invalid_char()
    {
        //Arrange

        //Act
        var res = FilesAndDirectoryInfoExtension.ContainsInvalidCharFromName("df@ù^^sfdf");

        //Assert
        Assert.False(res);
    }

    [Fact]
    public void EscapeInvalidPathFromName_escapes_invalid_char()
    {
        //Arrange

        //Act
        var res = FilesAndDirectoryInfoExtension.EscapeInvalidPathFromName("dfdsf/sdf");

        //Assert
        Assert.Equal("dfdsfsdf", res);
    }

    [Fact]
    public void EscapeInvalidPathFromName_escapes_invalid_char_slash()
    {
        //Arrange

        //Act
        var res = FilesAndDirectoryInfoExtension.EscapeInvalidPathFromName("dfdsf/sdf");

        //Assert
        Assert.Equal("dfdsfsdf", res);
    }

    [Fact]
    public void EscapeInvalidPathFromName_returns_false_if_file_does_not_contains_invalid_char()
    {
        //Arrange

        //Act
        var res = FilesAndDirectoryInfoExtension.EscapeInvalidPathFromName("df@ù^^sfdf");

        //Assert
        Assert.Equal("df@ù^^sfdf", res);
    }

    [Fact]
    public void IsValidDirectory_returns_true_for_valid_directory_path()
    {
        //Arrange

        //Act
        var res = FilesAndDirectoryInfoExtension.IsValidDirectory(_testDirectory.FullName);

        //Assert
        Assert.True(res);
    }

    [Fact]
    public void IsValidDirectory_returns_false_for_invalid_directory_path()
    {
        //Arrange

        //Act
        var res = FilesAndDirectoryInfoExtension.IsValidDirectory("?");

        //Assert
        Assert.False(res);
    }


    [Fact]
    public void AutoRenameDirectoryToAvoidDuplicate_returns_available_names()
    {
        //Arrange
        var newDirectory = _testDirectory.CreateSubdirectoryIfNotExist(Guid.NewGuid().ToString());
        var subDirectory = newDirectory.CreateSubdirectoryIfNotExist("dirTemp");

        try
        {
            for (var i = 2; i < 10; i++)
            {
                //Act
                var res = FilesAndDirectoryInfoExtension.AutoRenameDirectoryToAvoidDuplicate(subDirectory.FullName);
                Assert.False(res.Exists);
                res.CreateIfNotExist(); // create it for the next iteration

                //Assert
                Assert.Equal($"dirTemp({i})", res.Name);
            }
        }
        finally
        {
            newDirectory.DeleteIfExistAndWaitDeletion();
        }
    }

    [Fact]
    public void AutoRenameDirectoryToAvoidDuplicateWithCreateDirectory_returns_available_names_and_create_directories()
    {
        //Arrange
        var newDirectory = _testDirectory.CreateSubdirectoryIfNotExist(Guid.NewGuid().ToString());
        var subDirectory = newDirectory.CreateSubdirectoryIfNotExist("dirTemp");

        try
        {
            for (var i = 2; i < 10; i++)
            {
                //Act
                var res = FilesAndDirectoryInfoExtension.AutoRenameDirectoryToAvoidDuplicateWithCreateDirectory(subDirectory.FullName);
                Assert.True(res.Exists);

                //Assert
                Assert.Equal($"dirTemp({i})", res.Name);
            }
        }
        finally
        {
            newDirectory.DeleteIfExistAndWaitDeletion();
        }
    }

    [Fact]
    public void CopyTo_copy_content_and_does_not_alter_original_directoryInfo()
    {
        //Arrange
        var newDirectory = _testDirectory.CreateSubdirectoryIfNotExist(Guid.NewGuid().ToString());
        var subDirectory = newDirectory.CreateSubdirectoryIfNotExist("dirTemp");
        subDirectory.CreateFileIfNotExist("foo.txt");

        try
        {
            var copy = subDirectory.CopyTo(newDirectory.GetDirectory("subOther").FullName);

            Assert.True("dirTemp" == subDirectory.Name, "the original directoryinfo should not be altered");
            Assert.Equal("subOther", copy.Name);
            Assert.True(copy.TryGetFile("foo.txt", out _));
        }
        finally
        {
            newDirectory.DeleteIfExistAndWaitDeletion();
        }
    }


    [Fact]
    public void CopyToWithCheckLenght_lower_than_lenght()
    {
        //Arrange
        var newDirectory = _testDirectory.CreateSubdirectoryIfNotExist(Guid.NewGuid().ToString());
        var subDirectory = newDirectory.CreateSubdirectoryIfNotExist("dirTemp");
        subDirectory.CreateFileIfNotExist("foo.txt");

        try
        {
            var copy = subDirectory.CopyToWithCheckLenght(newDirectory.GetDirectory("subOther").FullName);

            Assert.True("dirTemp" == subDirectory.Name, "the original directoryinfo should not be altered");
            Assert.Equal("subOther", copy.Name);
            Assert.True(copy.TryGetFile("foo.txt", out _));
        }
        finally
        {
            newDirectory.DeleteIfExistAndWaitDeletion();
        }
    }

    [Fact]
    public void ReadAllText_returns_file_content_as_string()
    {
        //Arrange
        var file = _testDirectory.GetFile("testFile.txt");
        File.WriteAllText(file.FullName, "Lorem ipsum");

        //Act
        var res = file.ReadAllText();

        //Assert
        Assert.Equal("Lorem ipsum", res);
    }

    [Fact]
    public void DirectoryInfo_DeleteIfExist_return_false_if_already_deleted()
    {
        //Arrange
        var dir = _testDirectory.GetDirectory("tmpDir");

        //Act
        var res = dir.DeleteIfExist();

        //Assert
        Assert.False(res);
    }

    [Fact]
    public void DirectoryInfo_DeleteIfExist_return_true_if_deleted()
    {
        //Arrange
        var dir = _testDirectory.CreateSubdirectory("tmpDir");

        //Act
        var res = dir.DeleteIfExist();

        //Assert
        Assert.True(res);
    }

    [Fact]
    public void AppendTextLine_on_existing_file()
    {
        //Arrange
        _testDirectory.CleanDirectory();
        _testDirectory.CreateFileIfNotExist("testFile.txt");

        var fileCopy = _testDirectory.GetFile("testFile.txt").CopyTo(_testDirectory.GetFile("copy.txt").FullName);

        try
        {
            //Act
            fileCopy.AppendTextLine("toto");

            //Assert
            Assert.Equal($"toto{Environment.NewLine}", fileCopy.ReadAllText());
        }
        finally
        {
            fileCopy.DeleteIfExistAndWaitDeletion();
        }
    }

    [Fact]
    public void TryGetDirectory_returns_true_if_directory_exists()
    {
        //Arrange

        //Act
        var res = _testDirectory.Parent.TryGetDirectory(_testDirectory.Name, out var retrievedDirectory);

        //Assert
        Assert.True(res);
        Assert.Equal(_testDirectory.FullName, retrievedDirectory.FullName);
    }

    [Fact]
    public void TryGetDirectory_returns_false_if_directory_does_not_exists()
    {
        //Arrange

        //Act
        var res = _testDirectory.TryGetDirectory("foo", out _);

        //Assert
        Assert.False(res);
    }
}