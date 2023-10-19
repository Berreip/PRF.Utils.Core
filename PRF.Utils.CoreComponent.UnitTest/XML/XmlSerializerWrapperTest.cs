using System.IO;
using NUnit.Framework;
using PRF.Utils.CoreComponents.Extensions;
using PRF.Utils.CoreComponents.XML;

namespace PRF.Utils.CoreComponent.UnitTest.XML;

[TestFixture]
public class JsonSerializerWrapperTest
{
    private FileInfo _xmlSmallFile;
    private DirectoryInfo _testDirectory;

    [SetUp]
    public void TestInitialize()
    {
        // mock:
        _testDirectory = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, "XML"));

        _xmlSmallFile = _testDirectory.GetFile("testXml.xml");
        Assert.IsNotNull(_xmlSmallFile);
        Assert.IsTrue(_xmlSmallFile.Exists);
    }

    [Test]
    public void SerializeToXml_returns_correct_values()
    {
        //Configuration
        const string target = "<TestClassToSerialize><Id>75</Id><Name>Robert</Name></TestClassToSerialize>";
        var dataToSerialize = new TestClassToSerialize { Id = 75, Name = "Robert" };

        //Test
        var res = dataToSerialize.SerializeToXml();

        //Verify
        Assert.AreEqual(target, res);
    }
      
    /// <summary>
    /// Case 1: test that deserialization is correctly done in xml
    /// </summary>
    [Test]
    public void DeserializeFromXml()
    {
        //Configuration
        const string str = "<TestClassToSerialize><Id>75</Id><Name>Robert</Name></TestClassToSerialize>";

        //Test
        var res = str.DeserializeFromXml<TestClassToSerialize>();

        //Verify
        Assert.AreEqual("Robert", res.Name);
        Assert.AreEqual(75, res.Id);
    }
}

public sealed class TestClassToSerialize
{
    // ReSharper disable PropertyCanBeMadeInitOnly.Global
    public int Id { get; set; }
    public string Name { get; set; }
    // ReSharper restore PropertyCanBeMadeInitOnly.Global
}