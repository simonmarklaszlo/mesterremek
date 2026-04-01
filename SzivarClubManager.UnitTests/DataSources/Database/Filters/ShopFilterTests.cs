using Npgsql;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Models;

namespace SzivarClubManager.UnitTests.DataSources.Database.Filters;

public class ShopFilterTests : FilterTests<ShopFilter, Shop>
{
    private readonly ShopFilter Filter;

    public ShopFilterTests()
    {
        Filter = new ShopFilter
        {
            MinId = 1,
            MaxId = 10,
            Name = "Shop",
            Address = "Main Street",
            City = "Budapest",
            MinLatitude = 47,
            MaxLatitude = 48,
            MinLongitude = 19,
            MaxLongitude = 20
        };
    }

    public override void EmptyFilter_ShouldBeEmpty()
    {
        Assert.True(EmptyFilter.IsEmpty);

        Assert.Null(EmptyFilter.MinId);
        Assert.Null(EmptyFilter.MaxId);
        Assert.Null(EmptyFilter.Name);
        Assert.Null(EmptyFilter.Address);
        Assert.Null(EmptyFilter.City);
        Assert.Null(EmptyFilter.MinLatitude);
        Assert.Null(EmptyFilter.MaxLatitude);
        Assert.Null(EmptyFilter.MinLongitude);
        Assert.Null(EmptyFilter.MaxLongitude);
    }

    public override void Filter_ShouldBeNotEmpty()
    {
        Assert.False(Filter.IsEmpty);

        Assert.Equal(1, Filter.MinId);
        Assert.Equal(10, Filter.MaxId);
        Assert.Equal("Shop", Filter.Name);
        Assert.Equal("Main Street", Filter.Address);
        Assert.Equal("Budapest", Filter.City);
        Assert.Equal(47, Filter.MinLatitude);
        Assert.Equal(48, Filter.MaxLatitude);
        Assert.Equal(19, Filter.MinLongitude);
        Assert.Equal(20, Filter.MaxLongitude);
    }

    public override void Filter_ToString_ShouldReturnExpectedString()
    {
        const string expected = "Id=[1..10] Name=\"Shop\" Address=\"Main Street\" City=\"Budapest\" Latitude=[47..48] Longitude=[19..20]";

        string actual = Filter.ToString();

        Assert.Equal(expected, actual);
    }

    public override void Filter_CopyTo_ShouldCopyValues()
    {
        ShopFilter result = new();

        Filter.CopyTo(result);

        Assert.Equal(1, result.MinId);
        Assert.Equal(10, result.MaxId);
        Assert.Equal("Shop", result.Name);
        Assert.Equal("Main Street", result.Address);
        Assert.Equal("Budapest", result.City);
        Assert.Equal(47, result.MinLatitude);
        Assert.Equal(48, result.MaxLatitude);
        Assert.Equal(19, result.MinLongitude);
        Assert.Equal(20, result.MaxLongitude);
    }

    public override void Filter_ConstructParameterizedQuery_ShouldReturnExpectedString()
    {
        const string expected = """
                                WHERE id BETWEEN @MinId AND @MaxId AND name LIKE '%' || @Name || '%' AND address LIKE '%' || @Address || '%' AND city LIKE '%' || @City || '%' AND ST_Y(location) BETWEEN @MinLatitude AND @MaxLatitude AND ST_X(location) BETWEEN @MinLongitude AND @MaxLongitude
                                """;

        string actual = Filter.ConstructParameterizedQuery();

        Assert.Equal(expected, actual);
    }

    public override void Filter_AddParameters_ShouldAddExpectedParameters()
    {
        using NpgsqlCommand command = new NpgsqlCommand();

        Filter.AddParameters(command.Parameters);

        Assert.Equal(9, command.Parameters.Count);

        Assert.Equal("MinId", command.Parameters[0].ParameterName);
        Assert.Equal("MaxId", command.Parameters[1].ParameterName);
        Assert.Equal("Name", command.Parameters[2].ParameterName);
        Assert.Equal("Address", command.Parameters[3].ParameterName);
        Assert.Equal("City", command.Parameters[4].ParameterName);
        Assert.Equal("MinLatitude", command.Parameters[5].ParameterName);
        Assert.Equal("MaxLatitude", command.Parameters[6].ParameterName);
        Assert.Equal("MinLongitude", command.Parameters[7].ParameterName);
        Assert.Equal("MaxLongitude", command.Parameters[8].ParameterName);

        Assert.Equal(command.Parameters["MinId"].Value, Filter.MinId);
        Assert.Equal(command.Parameters["MaxId"].Value, Filter.MaxId);
        Assert.Equal(command.Parameters["Name"].Value, Filter.Name);
        Assert.Equal(command.Parameters["Address"].Value, Filter.Address);
        Assert.Equal(command.Parameters["City"].Value, Filter.City);
        Assert.Equal(command.Parameters["MinLatitude"].Value, Filter.MinLatitude);
        Assert.Equal(command.Parameters["MaxLatitude"].Value, Filter.MaxLatitude);
        Assert.Equal(command.Parameters["MinLongitude"].Value, Filter.MinLongitude);
        Assert.Equal(command.Parameters["MaxLongitude"].Value, Filter.MaxLongitude);
    }

    public override void Filter_Parse_ShouldRestoreValues()
    {
        string filterString = Filter.ToString();

        IFilter<Shop> parsed = ShopFilter.Parse(filterString);

        Assert.IsType<ShopFilter>(parsed);

        ShopFilter shopFilter = (ShopFilter)parsed;

        Assert.Equal(1, shopFilter.MinId);
        Assert.Equal(10, shopFilter.MaxId);
        Assert.Equal("Shop", shopFilter.Name);
        Assert.Equal("Main Street", shopFilter.Address);
        Assert.Equal("Budapest", shopFilter.City);
        Assert.Equal(47, shopFilter.MinLatitude);
        Assert.Equal(48, shopFilter.MaxLatitude);
        Assert.Equal(19, shopFilter.MinLongitude);
        Assert.Equal(20, shopFilter.MaxLongitude);
    }

    public override void Filter_Parse_ShouldParseCorrectly()
    {
        const string filterString = "Id=[1..10] Name=\"Shop\" Address=\"Main Street\" City=\"Budapest\" Latitude=[47..48] Longitude=[19..20]";

        IFilter<Shop> parsed = ShopFilter.Parse(filterString);

        Assert.IsType<ShopFilter>(parsed);

        ShopFilter shopFilter = (ShopFilter)parsed;

        Assert.Equal(1, shopFilter.MinId);
        Assert.Equal(10, shopFilter.MaxId);
        Assert.Equal("Shop", shopFilter.Name);
        Assert.Equal("Main Street", shopFilter.Address);
        Assert.Equal("Budapest", shopFilter.City);
        Assert.Equal(47, shopFilter.MinLatitude);
        Assert.Equal(48, shopFilter.MaxLatitude);
        Assert.Equal(19, shopFilter.MinLongitude);
        Assert.Equal(20, shopFilter.MaxLongitude);
    }

    [Fact]
    public void Filter_AddParameters_SingleValue_ShouldAddExpectedParameters()
    {
        ShopFilter filter = new ShopFilter()
        {
            MinId = 1,
            Name = "Shop",
            Address = "Main Street",
            City = "Budapest",
            MinLatitude = 47,
            MinLongitude = 19
        };

        using NpgsqlCommand command = new NpgsqlCommand();

        filter.AddParameters(command.Parameters);

        Assert.Equal(6, command.Parameters.Count);

        Assert.Equal("MinId", command.Parameters[0].ParameterName);
        Assert.Equal("Name", command.Parameters[1].ParameterName);
        Assert.Equal("Address", command.Parameters[2].ParameterName);
        Assert.Equal("City", command.Parameters[3].ParameterName);
        Assert.Equal("MinLatitude", command.Parameters[4].ParameterName);
        Assert.Equal("MinLongitude", command.Parameters[5].ParameterName);

        Assert.Equal(command.Parameters["MinId"].Value, filter.MinId);
        Assert.Equal(command.Parameters["Name"].Value, filter.Name);
        Assert.Equal(command.Parameters["Address"].Value, filter.Address);
        Assert.Equal(command.Parameters["City"].Value, filter.City);
        Assert.Equal(command.Parameters["MinLatitude"].Value, filter.MinLatitude);
        Assert.Equal(command.Parameters["MinLongitude"].Value, filter.MinLongitude);
    }
}