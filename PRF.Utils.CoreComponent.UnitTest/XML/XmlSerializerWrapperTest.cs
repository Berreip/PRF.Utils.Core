using System.IO;
using NUnit.Framework;
using PRF.Utils.CoreComponents.Extensions;
using PRF.Utils.CoreComponents.XML;

namespace PRF.Utils.CoreComponent.UnitTest.XML;

[TestFixture]
public class JsonSerializerWrapperTest
{
    private FileInfo _xmlsmallFile;
    private DirectoryInfo _testDirectory;

    [SetUp]
    public void TestInitialize()
    {
        // mock:
        _testDirectory = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, @"XML"));

        _xmlsmallFile = _testDirectory.GetFile(@"testXml.xml");
        Assert.IsNotNull(_xmlsmallFile);
        Assert.IsTrue(_xmlsmallFile.Exists);
    }

    [Test]
    public void SerializeToXml_returns_correct_values()
    {
        //Configuration
        const string target = @"<TestClassToSerialize><Id>75</Id><Name>Robert</Name></TestClassToSerialize>";
        var dataToSerialize = new TestClassToSerialize { Id = 75, Name = "Robert" };

        //Test
        var res = dataToSerialize.SerializeToXml();

        //Verify
        Assert.AreEqual(target, res);
    }
      
    /// <summary>
    /// Cas 1: test que la dé-sérialisation est correctement faite en xml
    /// </summary>
    [Test]
    public void DeserializeFromXml()
    {
        //Configuration
        var str = @"<TestClassToSerialize><Id>75</Id><Name>Robert</Name></TestClassToSerialize>";

        //Test
        var res = str.DeserializeFromXml<TestClassToSerialize>();

        //Verify
        Assert.AreEqual("Robert", res.Name);
        Assert.AreEqual(75, res.Id);
    }
}

public sealed class TestClassToSerialize
{
    public int Id { get; set; }
    public string Name { get; set; }
}