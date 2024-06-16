using System.Collections.Generic;
using PRF.Utils.CoreComponents.Extensions;
using System.Collections.Concurrent;

namespace PRF.Utils.CoreComponent.UnitTest.Extensions;

public sealed class ConcurrentDictionaryExtensionsTests
{
    [Fact]
    public void AddRangeDifferential_Dictionary_Empty_Test()
    {
        // Arrange
        var sut = new ConcurrentDictionary<int, object>();

        // Act 
        sut.AddRangeDifferential(new Dictionary<int, object>());

        // Assert
        Assert.Empty(sut);
    }

    [Fact]
    public void AddRangeDifferential_Dictionary_Test()
    {
        // Arrange
        var sut = new ConcurrentDictionary<int, object>();
        sut.TryAdd(23, new object());

        // Act 
        // ask to add an empty differential => we have to get an empty differential as a result
        sut.AddRangeDifferential(new Dictionary<int, object>());

        // Assert
        Assert.Empty(sut);
    }

    [Fact]
    public void AddRangeDifferential_Dictionary_Nominal_Test()
    {
        // Arrange
        var sut = new ConcurrentDictionary<int, object>();
        var addingDictionary = new Dictionary<int, object> { { 23, new object() } };

        // Act 
        sut.AddRangeDifferential(addingDictionary);

        // Assert
        Assert.Single(sut);
        Assert.True(sut.ContainsKey(23));
    }

    [Fact]
    public void AddRangeDifferential_Dictionary_Nominal__Both_Not_Empty_Test()
    {
        // Arrange
        var targetData = new object();
        var sut = new ConcurrentDictionary<int, object>();
        sut.TryAdd(23, new object());
        sut.TryAdd(25, targetData);

        var addingDictionary = new Dictionary<int, object>
        {
            { 26, new object() },
            { 25, new object() },
        };

        // Act 
        sut.AddRangeDifferential(addingDictionary);

        // Assert
        Assert.Equal(2, sut.Count);
        Assert.True(sut.ContainsKey(25));
        Assert.True(sut.ContainsKey(26));
        Assert.True(ReferenceEquals(sut[25], targetData));
    }
}