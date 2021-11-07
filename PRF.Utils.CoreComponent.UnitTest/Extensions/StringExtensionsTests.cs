using System.IO;
using NUnit.Framework;
using PRF.Utils.CoreComponents.Extensions;

namespace PRF.Utils.CoreComponent.UnitTest.Extensions
{
    [TestFixture]
    internal sealed class StringExtensionsTests
    {
        [Test]
        public void ReplaceCaseInsensitive_returns_correct_value_with_same_case()
        {
            //Arrange
            var str = "foo_content";

            //Act
            var res = str.ReplaceCaseInsensitive("oo_conten", "AAA");

            //Assert
            Assert.AreEqual("fAAAt", res);
        }

        [Test]
        public void ReplaceCaseInsensitive_returns_correct_value_with_different_case()
        {
            //Arrange
            var str = "foo_content";

            //Act
            var res = str.ReplaceCaseInsensitive("OO_CONTEN", "AAA");

            //Assert
            Assert.AreEqual("fAAAt", res);
        }
        
        [Test]
        public void ReplaceCaseInsensitive_returns_correct_value_if_not_found()
        {
            //Arrange
            var str = "foo_content";

            //Act
            var res = str.ReplaceCaseInsensitive("OO_EN", "AAA");

            //Assert
            Assert.AreEqual("foo_content", res);
        }
        
        [Test]
        public void GetBetween_returns_correct_value()
        {
            //Arrange
            var str = "foo_content";

            //Act
            var res = str.GetBetween("foo", "ontent");

            //Assert
            Assert.AreEqual("_c", res);
        }
        
        [Test]
        public void GetBetween_returns_correct_value_if_second_part_not_found()
        {
            //Arrange
            var str = "foo_content";

            //Act
            var res = str.GetBetween("foo", "ontentdsfgfdgdfg");

            //Assert
            Assert.IsNull(res);
        }
        
        [Test]
        public void GetBetween_returns_correct_value_if_first_part_not_found()
        {
            //Arrange
            var str = "foo_content";

            //Act
            var res = str.GetBetween("fofsdgdfgdgo", "onte");

            //Assert
            Assert.IsNull(res);
        }
        
        [Test]
        public void GetRelativePath_returns_correct_value_if_first_part_not_found()
        {
            //Arrange
            var dir = Path.Combine(Path.Combine(TestContext.CurrentContext.TestDirectory), "tmpDir");

            //Act
            var res = dir.GetRelativePath(TestContext.CurrentContext.TestDirectory);

            //Assert
            Assert.AreEqual("tmpDir", res);
        }
        
        [Test]
        public void RemoveEmptyLines_nominal()
        {
            //Arrange
            var str = @"oo
aaa

bb
";

            //Act
            var res = str.RemoveEmptyLines();

            //Assert
            Assert.AreEqual(
                @"oo
aaa
bb", res);
        } 
        
        [Test]
        public void ContainsInsensitive_returns_true_if_contains_same_case()
        {
            //Arrange
            var str = @"azerty78";

            //Act
            var res = str.ContainsInsensitive("erty7");

            //Assert
            Assert.IsTrue(res);
        }
        
        [Test]
        public void ContainsInsensitive_returns_true_if_contains_different_case()
        {
            //Arrange
            var str = @"azerty78";

            //Act
            var res = str.ContainsInsensitive("ERTY7");

            //Assert
            Assert.IsTrue(res);
        }
        
        [Test]
        public void ContainsInsensitive_returns_false_if_does_not_contains()
        {
            //Arrange
            var str = @"azerty78";

            //Act
            var res = str.ContainsInsensitive("aaa");

            //Assert
            Assert.IsFalse(res);
        }
        
        [Test]
        public void RemoveInvalidPathCharacters_nominal()
        {
            //Arrange
            var str = @"az/e?rty78";

            //Act
            var res = str.RemoveInvalidPathCharacters();

            //Assert
            Assert.AreEqual("azerty78", res);
        }
    }
}