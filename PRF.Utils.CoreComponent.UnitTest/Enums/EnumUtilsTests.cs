using System;
using PRF.Utils.CoreComponents.Enums;
// ReSharper disable UnusedMember.Global

namespace PRF.Utils.CoreComponent.UnitTest.Enums;

internal enum FooEnum
{
    Value0,
    DefaultValue,
    Value1,
    Value2,
}

public sealed class EnumUtilsTests
{
    [Fact]
    public void Constructor_assign_correct_initial_values()
    {
        //Arrange

        //Act
        var sut = EnumUtils.CreateConverterWithDefault(FooEnum.Value1);

        //Assert
        Assert.Equal(FooEnum.Value1, sut.DefaultValue);
    }

    [Fact]
    public void Convert_all_Value_from_string()
    {
        //Arrange
        var sut = EnumUtils.CreateConverterWithDefault(FooEnum.DefaultValue);

        //Act
        foreach (FooEnum enumValue in Enum.GetValues(typeof(FooEnum)))
        {
            var enumConverted = sut.Convert(enumValue.ToString());
            //Assert
            Assert.Equal(enumValue, enumConverted);
        }
    }

    [Fact]
    public void Convert_fallback_to_default_for_unknown_value()
    {
        //Arrange
        var sut = EnumUtils.CreateConverterWithDefault(FooEnum.DefaultValue);

        //Act
        // ReSharper disable once StringLiteralTypo
        var enumConverted = sut.Convert("fdgsdfg");
            
        //Assert
        Assert.Equal(FooEnum.DefaultValue, enumConverted);
    }
        
    [Fact]
    public void Convert_fallback_to_default_for_string_empty()
    {
        //Arrange
        var sut = EnumUtils.CreateConverterWithDefault(FooEnum.DefaultValue);

        //Act
        var enumConverted = sut.Convert(string.Empty);
            
        //Assert
        Assert.Equal(FooEnum.DefaultValue, enumConverted);
    }

    [Fact]
    public void Convert_fallback_to_default_for_null_input()
    {
        //Arrange
        var sut = EnumUtils.CreateConverterWithDefault(FooEnum.DefaultValue);

        //Act
        var enumConverted = sut.Convert(null);
            
        //Assert
        Assert.Equal(FooEnum.DefaultValue, enumConverted);
    }
        
    [Fact]
    public void Convert_all_Value_from_upper_string()
    {
        //Arrange
        var sut = EnumUtils.CreateConverterWithDefault(FooEnum.DefaultValue);

        //Act
        foreach (FooEnum enumValue in Enum.GetValues(typeof(FooEnum)))
        {
            var enumConverted = sut.Convert(enumValue.ToString().ToUpper());
            //Assert
            Assert.Equal(enumValue, enumConverted);
        }
    }

    [Fact]
    public void Convert_all_Value_case_sensitive_if_specified()
    {
        //Arrange
        var sut = EnumUtils.CreateConverterWithDefault(FooEnum.DefaultValue, StringComparer.CurrentCulture);

        //Act
        foreach (FooEnum enumValue in Enum.GetValues(typeof(FooEnum)))
        {
            var enumConverted = sut.Convert(enumValue.ToString().ToUpper());

            //Assert
            Assert.Equal(FooEnum.DefaultValue, enumConverted); // all default
        }
    }
}