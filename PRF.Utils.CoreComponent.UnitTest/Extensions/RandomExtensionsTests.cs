using System;
using NUnit.Framework;
using PRF.Utils.CoreComponents.Extensions;

namespace PRF.Utils.CoreComponent.UnitTest.Extensions
{
    [TestFixture]
    internal sealed class RandomExtensionsTests
    {
        private Random _rd;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _rd = new Random();
        }

        [Test]
        [Repeat(100)]
        public void NextBoolean()
        {
            //Arrange
            var counter = 0;

            //Act
            for (int i = 0; i < 50_000; i++)
            {
                if (_rd.NextBoolean())
                {
                    counter++;
                }
            }

            //Assert
            Assert.Greater(counter, 24000);
        }

        [Test]
        [Repeat(1_000)]
        public void NextNumberBetweenOneAndLessOne()
        {
            //Arrange

            //Act
            var res = _rd.NextNumberBetweenOneAndLessOne();

            //Assert
            Assert.GreaterOrEqual(res, -1);
            Assert.LessOrEqual(res, 1);
        }
    }
}