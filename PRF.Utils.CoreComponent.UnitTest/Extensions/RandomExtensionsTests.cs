using System;
using CommonUnitTest;
using PRF.Utils.CoreComponents.Extensions;

namespace PRF.Utils.CoreComponent.UnitTest.Extensions;

public sealed class RandomExtensionsTests
{
    private readonly Random _rd = new Random();

    [Theory]
    [Repeat(100)]
    public void NextBoolean(int _)
    {
        //Arrange
        var counter = 0;

        //Act
        for (var i = 0; i < 50_000; i++)
        {
            if (_rd.NextBoolean())
            {
                counter++;
            }
        }

        //Assert
        Assert.True(counter > 24000);
    }

    [Theory]
    [Repeat(1_000)]
    public void NextNumberBetweenOneAndLessOne(int _)
    {
        //Arrange

        //Act
        var res = _rd.NextNumberBetweenOneAndLessOne();

        //Assert
        Assert.True(res >= -1);
        Assert.True(res <= 1);
    }
}