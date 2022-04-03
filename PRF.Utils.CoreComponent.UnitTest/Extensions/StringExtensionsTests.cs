﻿using System;
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
            var str = @"az/erty78";

            //Act
            var res = str.RemoveInvalidPathCharacters();

            //Assert
            Assert.AreEqual("azerty78", res);
        }

        [Test]
        //Lower case
        [TestCase("", "", true)]
        [TestCase("", "toto", false)]
        [TestCase("toto", "different", false)]
        [TestCase("toto", "oto", false)]
        [TestCase("toto", "ot", false)]
        [TestCase("toto", "toto", true)]
        //With Upper case
        [TestCase("Toto", "toto", true)]
        [TestCase("TOto", "TO", true)]
        [TestCase("TOto", "OT", false)]
        [TestCase("TOto", "OTO", false)]
        [TestCase("TOTO", "ToT", true)]
        [TestCase("TOTO", "OTO", false)]
        public void StartsWithInsensitive_returns_expected_result(string input, string search, bool result)
        {
            //Arrange

            //Act
            var res = input.StartsWithInsensitive(search);

            //Assert
            Assert.AreEqual(result, res);
        }

        [Test]
        public void StartsWithInsensitive_throws_when_null_search()
        {
            //Arrange
            var str = "foo";

            //Act
            //Assert
            Assert.Throws<ArgumentNullException>(() => str.StartsWithInsensitive(null));
        }
        
        [Test]
        //Lower case
        [TestCase("", "", true)]
        [TestCase("", "toto", false)]
        [TestCase("toto", "different", false)]
        [TestCase("toto", "oto", true)]
        [TestCase("toto", "tot", false)]
        [TestCase("toto", "ot", false)]
        [TestCase("toto", "toto", true)]
        //With Upper case
        [TestCase("Toto", "otO", true)]
        [TestCase("tOTO", "tOt", false)]
        [TestCase("TOTO", "ot", false)]
        [TestCase("Toto", "TOTo", true)]
        public void EndsWithInsensitive_returns_expected_result(string input, string search, bool result)
        {
            //Arrange

            //Act
            var res = input.EndsWithInsensitive(search);

            //Assert
            Assert.AreEqual(result, res);
        }

        [Test]
        public void EndsWithInsensitive_throws_when_null_search()
        {
            //Arrange
            var str = "foo";

            //Act
            //Assert
            Assert.Throws<ArgumentNullException>(() => str.EndsWithInsensitive(null));
        }
    }
}