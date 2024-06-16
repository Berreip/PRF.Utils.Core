using System;
using System.Linq;
using PRF.Utils.CoreComponents.Extensions;

namespace PRF.Utils.CoreComponent.UnitTest.Extensions;

public sealed class ListFilteringExtensionTest
{
    private readonly Random _rd = new Random();

    [Fact]
    public void CapRandomized_filter_the_list()
    {
        //Arrange
        var initialList = Enumerable.Range(1, 1000).ToList();

        //Act
        initialList.CapRandomized(250);

        //Assert
        Assert.Equal(250, initialList.Count);
    }

    [Fact]
    public void GetRandomElement_returns_an_element_from_the_list()
    {
        //Arrange
        var initialList = Enumerable.Range(1, 1000).ToArray();

        //Act
        var res = initialList.GetRandomElement(_rd);

        //Assert
        Assert.True(res >= 0);
        Assert.True(res <= 1000);
    }
}