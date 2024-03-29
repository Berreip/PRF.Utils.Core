﻿using NUnit.Framework;
using System.Collections.Generic;
using PRF.Utils.CoreComponents.Extensions;

namespace PRF.Utils.CoreComponent.UnitTest.Extensions;

[TestFixture]
internal sealed class DictionaryExtensionsTests
{
    [Test]
    public void AddRangeDifferential_Dictionary_Empty_Test()
    {
        // Arrange
        var sut = new Dictionary<int, object>();

        // Act 
        sut.AddRangeDifferential(new Dictionary<int, object>());

        // Assert
        Assert.AreEqual(0, sut.Count);
    }

    [Test]
    public void AddRangeDifferential_Dictionary_Test()
    {
        // Arrange
        var sut = new Dictionary<int, object> { { 23, new object() } };

        // Act 
        // ask to add an empty differential => we have to get an empty differential as a result
        sut.AddRangeDifferential(new Dictionary<int, object>());

        // Assert
        Assert.AreEqual(0, sut.Count);
    }

    [Test]
    public void AddRangeDifferential_Dictionary_Nominal_Test()
    {
        // Arrange
        var sut = new Dictionary<int, object>();
        var addingDictionary = new Dictionary<int, object> { { 23, new object() } };

        // Act 
        sut.AddRangeDifferential(addingDictionary);

        // Assert
        Assert.AreEqual(1, sut.Count);
        Assert.IsTrue(sut.ContainsKey(23));
    }

    [Test]
    public void AddRangeDifferential_Dictionary_Nominal__Both_Not_Empty_Test()
    {
        // Arrange
        var targetData = new object();
        var sut = new Dictionary<int, object>
        {
            { 23, new object() },
            { 25, targetData },
        };

        var addingDictionary = new Dictionary<int, object>
        {
            { 26, new object() },
            { 25, new object() },
        };

        // Act 
        sut.AddRangeDifferential(addingDictionary);

        // Assert
        Assert.AreEqual(2, sut.Count);
        Assert.IsTrue(sut.ContainsKey(25));
        Assert.IsTrue(sut.ContainsKey(26));
        Assert.IsTrue(ReferenceEquals(sut[25], targetData));
    }
}