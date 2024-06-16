using PRF.Utils.CoreComponents.JSON;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace PRF.Utils.CoreComponent.UnitTest.JSON;


public class JsonSerializerWrapperTest
{
    /// <summary>
    /// Cas 1: test that the serialization is correctly done in Json
    /// </summary>
    [Fact]
    public void SerializeToJsonV1()
    {
        //Configuration
        const string target = """{"Id":75,"Name":"Robert"}""";
        var dataToSerialize = new TestClassToSerializeJson { Id = 75, Name = "Robert" };

        //Test
        var res = dataToSerialize.SerializeToJson();

        //Verify
        Assert.Equal(target, res);
    }

    /// <summary>
    /// Case 1: test that deserialization is correctly done in Json
    /// </summary>
    [Fact]
    public void DeserializeFromJson()
    {
        //Configuration
        const string str = """{"Id":75,"Name":"Robert"}""";

        //Test
        var res = str.DeserializeFromJson<TestClassToSerializeJson>();

        //Verify
        Assert.Equal("Robert", res.Name);
        Assert.Equal(75, res.Id);
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