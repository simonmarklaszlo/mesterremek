using SzivarClubManager.Datasources.Database.Filters;

namespace SzivarClubManager.UnitTests.DataSources.Database.Filters;

public class FilterValueHandlerTests
{
    [Fact]
    public void ToDisplayString_ShouldAddQuotes()
    {
        const string value = "test";

        string actual = FilterValueHandler.ToDisplayString(value);

        const string expected = "\"test\"";

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SplitToEntries_ShouldSplitOnSpaces()
    {
        const string value = "test test2";

        IEnumerable<string> actual = FilterValueHandler.SplitToEntries(value);

        string[] expected = ["test", "test2"];

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SplitToEntries_ShouldHandleQuotes()
    {
        const string value = "testKey1=\"testVal1\" testKey2=\"test val2\" testKey3=testVal3";

        string[] actual = FilterValueHandler.SplitToEntries(value).ToArray();

        string[] expected = ["testKey1=\"testVal1\"", "testKey2=\"test val2\"", "testKey3=testVal3"];

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ParseStringValue_Quoted_ShouldRemoveQuotes()
    {
        const string value = "\"test\"";

        string actual = FilterValueHandler.ParseStringValue(value);

        const string expected = "test";

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ParseStringValue_Unquoted_ShouldReturnSameValue()
    {
        const string value = "test";

        string actual = FilterValueHandler.ParseStringValue(value);

        const string expected = "test";

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ParseIntValues_Single_ShouldParse()
    {
        const string value = "5";

        (int?, int?) actual = FilterValueHandler.ParseIntValues(value);

        (int?, int?) expected = (5, 5);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ParseIntValues_Range_ShouldParse()
    {
        const string value = "[5..99]";

        (int?, int?) actual = FilterValueHandler.ParseIntValues(value);

        (int?, int?) expected = (5, 99);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ParseIntValues_Range_LeftSideOnly_ShouldParse()
    {
        const string value = "[5..]";

        (int?, int?) actual = FilterValueHandler.ParseIntValues(value);

        (int?, int?) expected = (5, null);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ParseIntValues_Range_RightSideOnly_ShouldParse()
    {
        const string value = "[..99]";

        (int?, int?) actual = FilterValueHandler.ParseIntValues(value);

        (int?, int?) expected = (null, 99);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ParseIntValues_Invalid_ShouldThrow()
    {
        const string value1 = "test";
        const string value2 = "[15..25";
        const string value3 = "15..25";
        const string value4 = "15  25";
        const string value5 = "15..test";

        Assert.Throws<FormatException>(() => FilterValueHandler.ParseIntValues(value1));
        Assert.Throws<FormatException>(() => FilterValueHandler.ParseIntValues(value2));
        Assert.Throws<FormatException>(() => FilterValueHandler.ParseIntValues(value3));
        Assert.Throws<FormatException>(() => FilterValueHandler.ParseIntValues(value4));
        Assert.Throws<FormatException>(() => FilterValueHandler.ParseIntValues(value5));
    }

    [Fact]
    public void ParseDoubleValues_Single_ShouldParse()
    {
        const string value = "5.2";

        (double?, double?) actual = FilterValueHandler.ParseDoubleValues(value);

        (double?, double?) expected = (5.2d, 5.2d);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ParseDoubleValues_Range_ShouldParse()
    {
        const string value = "[5.2..99.73]";

        (double?, double?) actual = FilterValueHandler.ParseDoubleValues(value);

        (double?, double?) expected = (5.2d, 99.73d);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ParseDoubleValues_Range_LeftSideOnly_ShouldParse()
    {
        const string value = "[5.2..]";

        (double?, double?) actual = FilterValueHandler.ParseDoubleValues(value);

        (double?, double?) expected = (5.2d, null);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ParseDoubleValues_Range_RightSideOnly_ShouldParse()
    {
        const string value = "[..99.73]";

        (double?, double?) actual = FilterValueHandler.ParseDoubleValues(value);

        (double?, double?) expected = (null, 99.73d);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ParseDoubleValues_Invalid_ShouldThrow()
    {
        const string value1 = "test";
        const string value2 = "[15.7..25.4";
        const string value3 = "15.7..25.4";
        const string value4 = "15.7  25.4";
        const string value5 = "15.7..test";

        Assert.Throws<FormatException>(() => FilterValueHandler.ParseDoubleValues(value1));
        Assert.Throws<FormatException>(() => FilterValueHandler.ParseDoubleValues(value2));
        Assert.Throws<FormatException>(() => FilterValueHandler.ParseDoubleValues(value3));
        Assert.Throws<FormatException>(() => FilterValueHandler.ParseDoubleValues(value4));
        Assert.Throws<FormatException>(() => FilterValueHandler.ParseDoubleValues(value5));
    }
}