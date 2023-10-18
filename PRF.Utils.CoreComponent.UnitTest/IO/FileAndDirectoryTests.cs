using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PRF.Utils.CoreComponents.IO;

namespace PRF.Utils.CoreComponent.UnitTest.IO;

public class FileAndDirectoryTests
{
    private IDirectoryInfo _testDirectory;

    [SetUp]
    public void TestInitialize()
    {
        // mock:
        _testDirectory = new DirectoryInfoWrapper(Path.Combine(TestContext.CurrentContext.TestDirectory, @"Extensions"));
        _testDirectory.CleanDirectory();
    }

    [TearDown]
    public void TestCleanup()
    {
        _testDirectory.DeleteIfExistAndWaitDeletion(TimeSpan.FromSeconds(5));
    }

    [Test]
    public void CreateDirectory_then_file_as_fluent()
    {
        //Arrange

        //Act
        var res = _testDirectory.CreateSubdirectoryIfNotExists("sub1").CreateSubdirectoryIfNotExists("sub2").CreateFileIfNotExists("foo.txt");

        //Assert
        Assert.IsNotNull(res);
        Assert.AreEqual(Path.Combine(_testDirectory.FullName, "sub1", "sub2", "foo.txt"), res.FullName);
    }

    [Test]
    public void CreateDirectory_then_file_does_not_throw_if_duplicated()
    {
        //Arrange

        //Act
        _ = _testDirectory.CreateSubdirectoryIfNotExists("sub1").CreateSubdirectoryIfNotExists("sub2").CreateFileIfNotExists("foo.txt");
        var res = _testDirectory.CreateSubdirectoryIfNotExists("sub1").CreateSubdirectoryIfNotExists("sub2").CreateFileIfNotExists("foo.txt");

        //Assert
        Assert.IsNotNull(res);
        Assert.AreEqual(Path.Combine(_testDirectory.FullName, "sub1", "sub2", "foo.txt"), res.FullName);
    }

    [Test]
    public void CreateDirectory_GetFile()
    {
        //Arrange

        //Act
        var res = _testDirectory.CreateSubdirectoryIfNotExists("sub1").GetFile("foo.txt");

        //Assert
        Assert.IsNotNull(res);
        Assert.IsFalse(res.Exists);
        Assert.IsFalse(res.ExistsExplicit);
        Assert.AreEqual(Path.Combine(_testDirectory.FullName, "sub1", "foo.txt"), res.FullName);
    }

    [Test]
    public void CreateFileIfNotExist_create_the_file_even_if_directory_did_not_exists()
    {
        //Arrange
        _testDirectory.DeleteIfExistAndWaitDeletion(TimeSpan.FromSeconds(5));

        //Act
        var file = _testDirectory.CreateFileIfNotExists("foo.txt");

        //Assert
        Assert.IsTrue(file.Exists);
    }

    [Test]
    public void ReadAllText_returns_file_content_as_string()
    {
        //Arrange
        var file = _testDirectory.CreateFileIfNotExists("testFile.txt");
        file.WriteAllText("Lorem ipsum");

        //Act
        var res = file.ReadAllText();

        //Assert
        Assert.AreEqual("Lorem ipsum", res);
    }

    [Test]
    public void ReadAllLines_returns_file_content_as_string()
    {
        //Arrange
        var file = _testDirectory.CreateFileIfNotExists("testFile.txt");
        file.WriteAllText("Lorem ipsum");

        //Act
        var res = file.ReadAllLines().Single();

        //Assert
        Assert.AreEqual("Lorem ipsum", res);
    }

    [Test]
    public void AppendAllText_write_file_content_as_string()
    {
        //Arrange
        var file = _testDirectory.CreateFileIfNotExists("testFile.txt");
        file.AppendAllText("Lorem ");
        file.AppendAllText("ipsum");

        //Act
        var res = file.ReadAllText();

        //Assert
        Assert.AreEqual("Lorem ipsum", res);
    }

    [Test]
    public void GetDirectory_when_directory_does_not_exists()
    {
        //Arrange

        //Act
        var res = _testDirectory.GetDirectory("sub1");

        //Assert
        Assert.IsNotNull(res);
        Assert.IsFalse(res.Exists);
        Assert.AreEqual(Path.Combine(_testDirectory.FullName, "sub1"), res.FullName);
    }

    [Test]
    public void Parent_is_valid()
    {
        //Arrange

        //Act
        var res = _testDirectory.CreateSubdirectoryIfNotExists("sub1").CreateSubdirectoryIfNotExists("sub2").CreateFileIfNotExists("foo.txt");

        //Assert
        Assert.IsNotNull(res);
        Assert.AreEqual(Path.Combine(_testDirectory.FullName, "sub1", "sub2"), res.Directory.FullName);
        Assert.AreEqual(Path.Combine(_testDirectory.FullName, "sub1"), res.Directory.Parent.FullName);
        Assert.AreEqual(_testDirectory.Root.FullName, res.Directory.Root.FullName);
    }

    [Test]
    public void CreateSubdirectory_nominal()
    {
        //Arrange

        //Act
        var res = _testDirectory.CreateSubdirectory("sub1");

        //Assert
        Assert.IsNotNull(res);
        Assert.AreEqual(Path.Combine(_testDirectory.FullName, "sub1"), res.FullName);
    }

    [Test]
    public void GetDirectories_nominal()
    {
        //Arrange
        _testDirectory.CreateSubdirectory("sub1");
        _testDirectory.CreateSubdirectory("sub2");

        //Act
        var subDir = _testDirectory.GetDirectories();

        //Assert
        Assert.AreEqual(2, subDir.Length);
    }

    [Test]
    public void GetFiles_nominal()
    {
        //Arrange
        _testDirectory.CreateSubdirectory("sub1");
        _testDirectory.CreateSubdirectory("sub2");
        _testDirectory.CreateFileIfNotExists("foo.txt");

        //Act
        var files = _testDirectory.GetFiles();

        //Assert
        Assert.AreEqual(1, files.Length);
    }

    [Test]
    public void GetFiles_nominal_SearchOption_AllDirectories()
    {
        //Arrange
        _testDirectory.CreateSubdirectory("sub1");
        var subDir = _testDirectory.CreateSubdirectory("sub2");
        subDir.CreateFileIfNotExists("foo.txt");

        //Act
        var files = _testDirectory.GetFiles("*.*", SearchOption.AllDirectories);

        //Assert
        Assert.AreEqual(1, files.Length);
    }

    [Test]
    public void IsEmpty_true()
    {
        //Arrange

        //Act

        //Assert
        Assert.IsTrue(_testDirectory.IsEmpty());
    }

    [Test]
    public void IsEmpty_false()
    {
        //Arrange
        _testDirectory.CreateSubdirectory("sub1");

        //Act

        //Assert
        Assert.IsFalse(_testDirectory.IsEmpty());
    }

    [Test]
    public void IsEmpty_false_file()
    {
        //Arrange
        _testDirectory.CreateFileIfNotExists("foo.txt");

        //Act

        //Assert
        Assert.IsFalse(_testDirectory.IsEmpty());
    }


    [Test]
    public void EnumerateDirectories_nominal()
    {
        //Arrange
        _testDirectory.CreateSubdirectory("sub1");
        _testDirectory.CreateSubdirectory("sub2");

        //Act
        var subDir = _testDirectory.EnumerateDirectories();

        //Assert
        Assert.AreEqual(2, subDir.Count());
    }

    [Test]
    public void EnumerateFiles_nominal()
    {
        //Arrange
        _testDirectory.CreateSubdirectory("sub1");
        _testDirectory.CreateSubdirectory("sub2");
        _testDirectory.CreateFileIfNotExists("foo.txt");

        //Act
        var files = _testDirectory.EnumerateFiles();

        //Assert
        Assert.AreEqual(1, files.Count());
    }

    [Test]
    public void EnumerateFiles_nominal_SearchOption_AllDirectories()
    {
        //Arrange
        _testDirectory.CreateSubdirectory("sub1");
        var subDir = _testDirectory.CreateSubdirectory("sub2");
        subDir.CreateFileIfNotExists("foo.txt");

        //Act
        var files = _testDirectory.EnumerateFiles("*.*", SearchOption.AllDirectories);

        //Assert
        Assert.AreEqual(1, files.Count());
    }

    [Test]
    public void DeleteIfExist_when_does_not_exists()
    {
        //Arrange
        var file = _testDirectory.GetFile("foo.txt");

        //Act
        var res = file.DeleteIfExist();

        //Assert
        Assert.IsFalse(res);
        Assert.IsFalse(file.ExistsExplicit);
    }
   
    [Test]
    public void DeleteIfExist_when_does_exists()
    {
        //Arrange
        var file = _testDirectory.CreateFileIfNotExists("foo.txt");

        //Act
        var res = file.DeleteIfExist();

        //Assert
        Assert.IsTrue(res);
        Assert.IsFalse(file.ExistsExplicit);
    }
    
    [Test]
    public void File_State_tests()
    {
        //Arrange

        //Act
        var file = _testDirectory.CreateFileIfNotExists("foo.txt");

        //Assert
        Assert.AreEqual(_testDirectory.FullName, file.DirectoryName);
        Assert.IsFalse(file.IsReadOnly);
        Assert.AreEqual(0, file.Length);
    }
}