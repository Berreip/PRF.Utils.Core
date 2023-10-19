using System;
using NUnit.Framework;
using System.Collections.Generic;
using PRF.Utils.CoreComponents.Extensions;
using System.Linq;

namespace PRF.Utils.CoreComponent.UnitTest.Extensions;

[TestFixture]
internal sealed class LinqExtensionsTests
{
    [Test]
    public void SplitInChunksOf_Nominal()
    {
        //Arrange
        var sut = new List<int> { 1, 2, 3, 4, 5, 6 };

        //Act
        var res = sut.SplitInChunksOf2(3).ToArray();

        //Assert
        // if we split the current list in chunks of 3 elements, we will get 2 of them
        Assert.AreEqual(2, res.Length);
        Assert.AreEqual(3, res[0].Count());
        Assert.AreEqual(3, res[1].Count());
    }

    [Test]
    public void SplitInChunksOf_Nominal_Not_Complete()
    {
        //Arrange
        var sut = new List<int> { 1, 2, 3, 4, 5, 6 };

        //Act
        var res = sut.SplitInChunksOf2(4).ToArray();

        //Assert
        // if we split the current list in chunks of 4 elements, we will get 2 of them
        Assert.AreEqual(2, res.Length);
        Assert.AreEqual(4, res[0].Count());
        Assert.AreEqual(2, res[1].Count());
    }

    [Test]
    public void SplitInChunksOf_throws_when_items_null()
    {
        //Arrange

        //Act
        // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
        Assert.Throws<ArgumentNullException>(() => ((List<int>)null).SplitInChunksOf2(6).ToArray());

        //Assert

    }
        
    [Test]
    public void SplitInChunksOf_throws_when_partition_size_lower_than_one()
    {
        //Arrange

        //Act
        // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
        Assert.Throws<ArgumentException>(() => new List<int>().SplitInChunksOf2(0).ToArray());

        //Assert
    }

}