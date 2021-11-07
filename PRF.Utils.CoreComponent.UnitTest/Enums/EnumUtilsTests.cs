using System;
using NUnit.Framework;
using PRF.Utils.CoreComponents.Enums;

namespace PRF.Utils.CoreComponent.UnitTest.Enums
{
    internal enum FooEnum
    {
        Value0,
        DefaultValue,
        Value1,
        Value2
    }

    [TestFixture]
    internal sealed class EnumUtilsTests
    {
        [Test]
        public void Constructor_assign_correct_initial_values()
        {
            //Arrange

            //Act
            var sut = EnumUtils.CreateConverterWithDefault(FooEnum.Value1);

            //Assert
            Assert.AreEqual(FooEnum.Value1, sut.DefaultValue);
        }

        [Test]
        public void Convert_all_Value_from_string()
        {
            //Arrange
            var sut = EnumUtils.CreateConverterWithDefault(FooEnum.DefaultValue);

            //Act
            foreach (FooEnum enumValue in Enum.GetValues(typeof(FooEnum)))
            {
                var enumConverted = sut.Convert(enumValue.ToString());
                //Assert
                Assert.AreEqual(enumValue, enumConverted);
            }
        }

        [Test]
        public void Convert_fallback_to_default_for_unknown_value()
        {
            //Arrange
            var sut = EnumUtils.CreateConverterWithDefault(FooEnum.DefaultValue);

            //Act
            var enumConverted = sut.Convert("fdgsdfg");
            
            //Assert
            Assert.AreEqual(FooEnum.DefaultValue, enumConverted);
        }
        
        [Test]
        public void Convert_fallback_to_default_for_string_empty()
        {
            //Arrange
            var sut = EnumUtils.CreateConverterWithDefault(FooEnum.DefaultValue);

            //Act
            var enumConverted = sut.Convert(string.Empty);
            
            //Assert
            Assert.AreEqual(FooEnum.DefaultValue, enumConverted);
        }

        [Test]
        public void Convert_fallback_to_default_for_null_input()
        {
            //Arrange
            var sut = EnumUtils.CreateConverterWithDefault(FooEnum.DefaultValue);

            //Act
            var enumConverted = sut.Convert(null);
            
            //Assert
            Assert.AreEqual(FooEnum.DefaultValue, enumConverted);
        }
        
        [Test]
        public void Convert_all_Value_from_upper_string()
        {
            //Arrange
            var sut = EnumUtils.CreateConverterWithDefault(FooEnum.DefaultValue);

            //Act
            foreach (FooEnum enumValue in Enum.GetValues(typeof(FooEnum)))
            {
                var enumConverted = sut.Convert(enumValue.ToString().ToUpper());
                //Assert
                Assert.AreEqual(enumValue, enumConverted);
            }
        }

        [Test]
        public void Convert_all_Value_case_sensitive_if_specified()
        {
            //Arrange
            var sut = EnumUtils.CreateConverterWithDefault(FooEnum.DefaultValue, StringComparer.CurrentCulture);

            //Act
            foreach (FooEnum enumValue in Enum.GetValues(typeof(FooEnum)))
            {
                var enumConverted = sut.Convert(enumValue.ToString().ToUpper());

                //Assert
                Assert.AreEqual(enumConverted, FooEnum.DefaultValue); // all default
            }
        }
    }
}