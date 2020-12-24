using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PRF.Utils.CoreComponents.JSON;

namespace PRF.Utils.CoreComponent.UnitTest.JSON
{
    [TestClass]
    public class JsonSerializerWrapperTest
    {
        /// <summary>
        /// Cas 1: test que la sérialisation est correctement faite en Json
        /// </summary>
        [TestMethod]
        public void SerializeToJsonV1()
        {
            //Configuration
            var target = @"{""Id"":75,""Name"":""Robert""}";
            var dataToSerialize = new TestClassToSerializeJson { Id = 75, Name = "Robert" };

            //Test
            var res = dataToSerialize.SerializeToJson();

            //Verify
            Assert.AreEqual(target, res);
        }

        /// <summary>
        /// Cas 1: test que la dé-sérialisation est correctement faite en Json
        /// </summary>
        [TestMethod]
        public void DeserializeFromJson()
        {
            //Configuration
            var str = @"{""Id"":75,""Name"":""Robert""}";

            //Test
            var res = str.DeserializeFromJson<TestClassToSerializeJson>();

            //Verify
            Assert.AreEqual("Robert", res.Name);
            Assert.AreEqual(75, res.Id);
        }

        [Ignore]
        [TestMethod]
        public void SerializeToJsonV2()
        {
            //Configuration
            var dataToSerialize = new TestClassToSerializeJson { Id = 75, Name = "Robert" };
            var array = new[] { dataToSerialize };

            var json = new List<TestClassToSerializeJson> { dataToSerialize, dataToSerialize };
            const int upper = 100_000;

            //Test
            var watch = Stopwatch.StartNew();
            for (int i = 0; i < upper; i++)
            {
#pragma warning disable 618
                var res = json.SerializeToJsonWithDataContractJsonSerializer();
#pragma warning restore 618
            }
            watch.Stop();
            var t1 = watch.Elapsed;

            watch.Restart();
            for (int i = 0; i < upper; i++)
            {
                var res = json.SerializeToJson();
            }
            watch.Stop();
            var t2 = watch.Elapsed;
           

            //Verify
            Assert.Fail($"t1 = {t1.TotalMilliseconds} ms - t2 = {t2.TotalMilliseconds} ms");
            //Assert.AreEqual(target, res);
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


}