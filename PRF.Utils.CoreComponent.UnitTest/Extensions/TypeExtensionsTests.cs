﻿using System.Linq;
using PRF.Utils.CoreComponents.Extensions;

// ReSharper disable UnusedMember.Local

namespace PRF.Utils.CoreComponent.UnitTest.Extensions;

public sealed class TypeExtensionsTests
{
    [Fact]
    public void GetPublicProperties_nominal()
    {
        //Arrange
        //Act
        var res = typeof(TmpClass).GetPublicProperties().ToArray();

        //Assert
        Assert.Single(res);
        Assert.Equal("PublicProp", res[0].Name);
    }

    [Fact]
    public void IsSubclassOfRawGeneric_returns_false_if_not_derived_from_generic()
    {
        //Arrange
        //Act
        var res = typeof(TmpClass).IsSubclassOfRawGeneric(typeof(GenericTmp<>));

        //Assert
        Assert.False(res);
    }

    [Fact]
    public void IsSubclassOfRawGeneric_returns_true_if_derived_from_generic_given_type()
    {
        //Arrange
        //Act
        var res = typeof(GenericTmpChild).IsSubclassOfRawGeneric(typeof(GenericTmp<>));

        //Assert
        Assert.True(res);
    }

    [Fact]
    public void GetPublicProperties_WhenConcreteType_ShouldReturnProperties()
    {
        // Arrange
        var concreteType = typeof(ConcreteClass);

        // Act
        var properties = concreteType.GetPublicProperties();

        // Assert
        Assert.NotEmpty(properties);
    }

    [Fact]
    public void GetPublicProperties_WhenInterfaceType_ShouldReturnPropertiesFromHierarchy()
    {
        // Arrange
        var interfaceType = typeof(IConcreteClassInterface);

        // Act
        var properties = interfaceType.GetPublicProperties();

        // Assert
        Assert.NotEmpty(properties);
    }

    [Fact]
    public void GetPublicProperties_WhenInvalidType_ShouldThrowException()
    {
        // Arrange
        var invalidType = typeof(InvalidType);

        // Act & Assert
        var properties = invalidType.GetPublicProperties();

        // Assert
        Assert.Empty(properties);
    }

    private class ConcreteClass
    {
        public int Property1 { get; set; }
        public string Property2 { get; set; }
    }

    private interface IConcreteClassInterface
    {
        int InterfaceProperty { get; set; }
    }

    private class InvalidType
    {
        // Invalid type without properties
    }


    private sealed class TmpClass
    {
        public int PublicProp { get; set; }
        private int PrivateProp { get; set; }
    }

    private sealed class GenericTmpChild : GenericTmp<object>
    {
    }

    // ReSharper disable once UnusedTypeParameter
    private class GenericTmp<T>
    {
    }
}