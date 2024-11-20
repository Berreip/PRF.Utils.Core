using System;
using System.Collections.Generic;
using PRF.Utils.CoreComponents.Extensions;
using System.Linq;

namespace PRF.Utils.CoreComponent.UnitTest.Extensions;

public sealed class LinqExtensionsTests
{
    [Fact]
    public void SplitInChunksOf_Nominal()
    {
        //Arrange
        var sut = new List<int> { 1, 2, 3, 4, 5, 6 };

        //Act
        var res = sut.SplitInChunksOf(3).ToArray();

        //Assert
        // if we split the current list in chunks of 3 elements, we will get 2 of them
        Assert.Equal(2, res.Length);
        Assert.Equal(3, res[0].Count());
        Assert.Equal(3, res[1].Count());
    }

    [Fact]
    public void SplitInChunksOf_Nominal_Not_Complete()
    {
        //Arrange
        var sut = new List<int> { 1, 2, 3, 4, 5, 6 };

        //Act
        var res = sut.SplitInChunksOf(4).ToArray();

        //Assert
        // if we split the current list in chunks of 4 elements, we will get 2 of them
        Assert.Equal(2, res.Length);
        Assert.Equal(4, res[0].Count());
        Assert.Equal(2, res[1].Count());
    }

    [Fact]
    public void SplitInChunksOf_throws_when_items_null()
    {
        //Arrange

        //Act
        // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
        Assert.Throws<ArgumentNullException>(() => ((List<int>)null).SplitInChunksOf(6).ToArray());

        //Assert

    }
        
    [Fact]
    public void SplitInChunksOf_throws_when_partition_size_lower_than_one()
    {
        //Arrange

        //Act
        // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
        Assert.Throws<ArgumentException>(() => new List<int>().SplitInChunksOf(0).ToArray());

        //Assert
    }

}