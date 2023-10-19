using NUnit.Framework;
using PRF.Utils.CoreComponents.JSON;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace PRF.Utils.CoreComponent.UnitTest.JSON;

[TestFixture]
public class JsonSerializerWrapperTest
{
    /// <summary>
    /// Cas 1: test that the serialization is correctly done in Json
    /// </summary>
    [Test]
    public void SerializeToJsonV1()
    {
        //Configuration
        const string target = """{"Id":75,"Name":"Robert"}""";
        var dataToSerialize = new TestClassToSerializeJson { Id = 75, Name = "Robert" };

        //Test
        var res = dataToSerialize.SerializeToJson();

        //Verify
        Assert.AreEqual(target, res);
    }

    /// <summary>
    /// Case 1: test that deserialization is correctly done in Json
    /// </summary>
    [Test]
    public void DeserializeFromJson()
    {
        //Configuration
        const string str = """{"Id":75,"Name":"Robert"}""";

        //Test
        var res = str.DeserializeFromJson<TestClassToSerializeJson>();

        //Verify
        Assert.AreEqual("Robert", res.Name);
        Assert.AreEqual(75, res.Id);
    }
}

//[DataContract]
public class TestClassToSerializeJson
{
    //[DataMember]
    public int Id { get; set; }

    //[DataMember]
    public string Name { get; set; }
}