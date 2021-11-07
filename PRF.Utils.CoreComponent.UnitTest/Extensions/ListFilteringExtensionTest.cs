using System;
using System.Linq;
using NUnit.Framework;
using PRF.Utils.CoreComponents.Extensions;

namespace PRF.Utils.CoreComponent.UnitTest.Extensions
{
    [TestFixture]
    internal sealed class ListFilteringExtensionTest
    {
        private Random _rd;

        [SetUp]
        public void TestInitialize()
        {
            _rd = new Random();
        }

        [Test]
        public void CapRandomized_filter_the_list()
        {
            //Arrange
            var initialList = Enumerable.Range(1, 1000).ToList();

            //Act
            initialList.CapRandomized(250);

            //Assert
            Assert.AreEqual(250, initialList.Count);
        }

        [Test]
        public void GetRandomElement_returns_an_element_from_the_list()
        {
            //Arrange
            var initialList = Enumerable.Range(1, 1000).ToArray();

            //Act
            var res = initialList.GetRandomElement(_rd);

            //Assert
            Assert.GreaterOrEqual(res, 0);
            Assert.LessOrEqual(res, 1000);
        }
    }
}