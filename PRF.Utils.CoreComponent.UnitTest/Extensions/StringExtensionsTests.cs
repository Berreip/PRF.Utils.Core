using System;
using CommonUnitTest;
using PRF.Utils.CoreComponents.Extensions;
// ReSharper disable StringLiteralTypo

namespace PRF.Utils.CoreComponent.UnitTest.Extensions;

public sealed class StringExtensionsTests
{
    [Fact]
    public void ReplaceCaseInsensitive_returns_correct_value_with_same_case()
    {
        //Arrange
        const string str = "foo_content";

        //Act
        var res = str.ReplaceCaseInsensitive("oo_conten", "AAA");

        //Assert
        Assert.Equal("fAAAt", res);
    }

    [Fact]
    public void ReplaceCaseInsensitive_returns_correct_value_with_different_case()
    {
        //Arrange
        const string str = "foo_content";

        //Act
        var res = str.ReplaceCaseInsensitive("OO_CONTEN", "AAA");

        //Assert
        Assert.Equal("fAAAt", res);
    }
        
    [Fact]
    public void ReplaceCaseInsensitive_returns_correct_value_if_not_found()
    {
        //Arrange
        const string str = "foo_content";

        //Act
        var res = str.ReplaceCaseInsensitive("OO_EN", "AAA");

        //Assert
        Assert.Equal("foo_content", res);
    }
        
    [Fact]
    public void GetBetween_returns_correct_value()
    {
        //Arrange
        const string str = "foo_content";

        //Act
        var res = str.GetBetween("foo", "ontent");

        //Assert
        Assert.Equal("_c", res);
    }
        
    [Fact]
    public void GetBetween_returns_correct_value_if_second_part_not_found()
    {
        //Arrange
        const string str = "foo_content";

        //Act
        var res = str.GetBetween("foo", "ontentdsfgfdgdfg");

        //Assert
        Assert.Null(res);
    }
        
    [Fact]
    public void GetBetween_returns_correct_value_if_first_part_not_found()
    {
        //Arrange
        const string str = "foo_content";

        //Act
        var res = str.GetBetween("fofsdgdfgdgo", "onte");

        //Assert
        Assert.Null(res);
    }
        
    [Fact]
    public void GetRelativePath_returns_correct_value_if_first_part_not_found()
    {
        //Arrange
        var directoryInfo = UnitTestFolder.Get("tmpDir");
        var dir = directoryInfo.FullName;

        //Act
        var res = dir.GetRelativePath(directoryInfo.Parent!.FullName);

        //Assert
        Assert.Equal("tmpDir", res);
    }
        
    [Fact]
    public void RemoveEmptyLines_nominal()
    {
        //Arrange
        const string str = """
                           oo
                           aaa

                           bb

                           """;

        //Act
        var res = str.RemoveEmptyLines();

        //Assert
        Assert.Equal(
            """
            oo
            aaa
            bb
            """, res);
    } 
        
    [Fact]
    public void ContainsInsensitive_returns_true_if_contains_same_case()
    {
        //Arrange
        const string str = "azerty78";

        //Act
        var res = str.ContainsInsensitive("erty7");

        //Assert
        Assert.True(res);
    }
        
    [Fact]
    public void ContainsInsensitive_returns_true_if_contains_different_case()
    {
        //Arrange
        const string str = "azerty78";

        //Act
        var res = str.ContainsInsensitive("ERTY7");

        //Assert
        Assert.True(res);
    }
        
    [Fact]
    public void ContainsInsensitive_returns_false_if_does_not_contains()
    {
        //Arrange
        const string str = "azerty78";

        //Act
        var res = str.ContainsInsensitive("aaa");

        //Assert
        Assert.False(res);
    }
        
    [Fact]
    public void RemoveInvalidPathCharacters_nominal()
    {
        //Arrange
        const string str = "az/erty78";

        //Act
        var res = str.RemoveInvalidPathCharacters();

        //Assert
        Assert.Equal("azerty78", res);
    }

    [Theory]
    //Lower case
    [InlineData("", "", true)]
    [InlineData("", "toto", false)]
    [InlineData("toto", "different", false)]
    [InlineData("toto", "oto", false)]
    [InlineData("toto", "ot", false)]
    [InlineData("toto", "toto", true)]
    //With Upper case
    [InlineData("Toto", "toto", true)]
    [InlineData("TOto", "TO", true)]
    [InlineData("TOto", "OT", false)]
    [InlineData("TOto", "OTO", false)]
    [InlineData("TOTO", "ToT", true)]
    [InlineData("TOTO", "OTO", false)]
    public void StartsWithInsensitive_returns_expected_result(string input, string search, bool result)
    {
        //Arrange

        //Act
        var res = input.StartsWithInsensitive(search);

        //Assert
        Assert.Equal(result, res);
    }

    [Fact]
    public void StartsWithInsensitive_throws_when_null_search()
    {
        //Arrange
        const string str = "foo";

        //Act
        //Assert
        Assert.Throws<ArgumentNullException>(() => str.StartsWithInsensitive(null));
    }
        
    [Theory]
    //Lower case
    [InlineData("", "", true)]
    [InlineData("", "toto", false)]
    [InlineData("toto", "different", false)]
    [InlineData("toto", "oto", true)]
    [InlineData("toto", "tot", false)]
    [InlineData("toto", "ot", false)]
    [InlineData("toto", "toto", true)]
    //With Upper case
    [InlineData("Toto", "otO", true)]
    [InlineData("tOTO", "tOt", false)]
    [InlineData("TOTO", "ot", false)]
    [InlineData("Toto", "TOTo", true)]
    public void EndsWithInsensitive_returns_expected_result(string input, string search, bool result)
    {
        //Arrange

        //Act
        var res = input.EndsWithInsensitive(search);

        //Assert
        Assert.Equal(result, res);
    }

    [Fact]
    public void EndsWithInsensitive_throws_when_null_search()
    {
        //Arrange
        const string str = "foo";

        //Act
        //Assert
        Assert.Throws<ArgumentNullException>(() => str.EndsWithInsensitive(null));
    }
        
    [Theory]
    // equals
    [InlineData("Toto", "toto", true)]
    [InlineData("tOTO", "TOTO", true)]
    [InlineData("toto", "TOTo", true)]
    [InlineData("TOTO", "TOTo", true)]
    // not Equals
    [InlineData("TOTO", "ot", false)]
    [InlineData("TOTO", "TOT", false)]
    [InlineData("TOTO", "OTO", false)]
    [InlineData("TOTO", "TOTOTOTO", false)]
    // accents => not equals 
    [InlineData("totoe", "totoé", false)]
    [InlineData("totoe", "totôe", false)]
    public void EqualsInsensitive_returns_expected_result(string input, string match, bool expected)
    {
        //Arrange

        //Act
        var res = input.EqualsInsensitive(match);

        //Assert
        Assert.Equal(expected, res);
    }
        
    [Theory]
    // equals
    [InlineData("Toto", "toto", true)]
    [InlineData("tOTO", "TOTO", true)]
    [InlineData("toto", "TOTo", true)]
    [InlineData("TOTO", "TOTo", true)]
    // not Equals
    [InlineData("TOTO", "ot", false)]
    [InlineData("TOTO", "TOT", false)]
    [InlineData("TOTO", "OTO", false)]
    [InlineData("TOTO", "TOTOTOTO", false)]
    // accents => equals 
    [InlineData("totoe", "totoé", true)]
    [InlineData("totoe", "totôe", true)]
    public void EqualsInsensitiveAndIgnoreAccents_returns_expected_result(string input, string match, bool expected)
    {
        //Arrange

        //Act
        var res = input.EqualsInsensitiveAndIgnoreAccents(match);

        //Assert
        Assert.Equal(expected, res);
    }
}