using Microsoft.VisualStudio.TestTools.UnitTesting;
using PRF.Utils.CoreComponents.XML;

namespace PRF.Utils.CoreComponent.UnitTest.XML
{
    [TestClass]
    public class JsonSerializerWrapperTest
    {
        /// <summary>
        /// Cas 1: test que la sérialisation est correctement faite en xml
        /// </summary>
        [TestMethod]
        public void SerializeToXmlV1()
        {
            //Configuration
            var target = @"<TestClassToSerialize><Id>75</Id><Name>Robert</Name></TestClassToSerialize>";
            var dataToSerialize = new TestClassToSerialize{Id = 75, Name = "Robert"};

            //Test
            var res = dataToSerialize.SerializeToXml();

            //Verify
            Assert.AreEqual(target, res);
        }

        /// <summary>
        /// Cas 1: test que la dé-sérialisation est correctement faite en xml
        /// </summary>
        [TestMethod]
        public void DeserializeFromXml()
        {
            //Configuration
            var str = @"<TestClassToSerialize><Id>75</Id><Name>Robert</Name></TestClassToSerialize>";

            //Test
            var res = str.DeserializeFromXML<TestClassToSerialize>();

            //Verify
            Assert.AreEqual("Robert", res.Name);
            Assert.AreEqual(75, res.Id);
        }
    }

    public class TestClassToSerialize
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
