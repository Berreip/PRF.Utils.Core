using System;
using System.IO;
using CommonUnitTest;
using PRF.Utils.CoreComponents.Extensions;
using PRF.Utils.CoreComponents.XML;

namespace PRF.Utils.CoreComponent.UnitTest.XML;

public class JsonSerializerWrapperTest
{
    public JsonSerializerWrapperTest()
    {
        // check that the provided test file exists
        var xmlSmallFile = UnitTestFolder.Get("XML").GetFile("testXml.xml");
        Assert.NotNull(xmlSmallFile);
        Assert.True(xmlSmallFile.Exists);
    }


    [Fact]
    public void SerializeToXml_returns_correct_values()
    {
        //Configuration
        const string target = "<TestClassToSerialize><Id>75</Id><Name>Robert</Name></TestClassToSerialize>";
        var dataToSerialize = new TestClassToSerialize { Id = 75, Name = "Robert" };

        //Test
        var res = dataToSerialize.SerializeToXml();

        //Verify
        Assert.Equal(target, res);
    }
      
    /// <summary>
    /// Case 1: test that deserialization is correctly done in xml
    /// </summary>
    [Fact]
    public void DeserializeFromXml()
    {
        //Configuration
        const string str = "<TestClassToSerialize><Id>75</Id><Name>Robert</Name></TestClassToSerialize>";

        //Test
        var res = str.DeserializeFromXml<TestClassToSerialize>();

        //Verify
        Assert.Equal("Robert", res.Name);
        Assert.Equal(75, res.Id);
    }
}

public sealed class TestClassToSerialize
{
    // ReSharper disable PropertyCanBeMadeInitOnly.Global
    public int Id { get; set; }
    public string Name { get; set; }
    // ReSharper restore PropertyCanBeMadeInitOnly.Global
}