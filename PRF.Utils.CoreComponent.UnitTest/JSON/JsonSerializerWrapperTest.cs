﻿using NUnit.Framework;
using PRF.Utils.CoreComponents.JSON;

namespace PRF.Utils.CoreComponent.UnitTest.JSON
{
    [TestFixture]
    public class JsonSerializerWrapperTest
    {
        /// <summary>
        /// Cas 1: test que la sérialisation est correctement faite en Json
        /// </summary>
        [Test]
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
        [Test]
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