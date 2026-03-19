using Npgsql;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Models;

namespace SzivarClubManager.UnitTests.DataSources.Database.Filters;

public class CigarFilterTests : FilterTests<CigarFilter, Cigar>
{
    private readonly CigarFilter Filter;

    public CigarFilterTests()
    {
        Filter = new CigarFilter
        {
            MinId = 1,
            MaxId = 10,
            Name = "Cigar"
        };
    }

    public override void EmptyFilter_ShouldBeEmpty()
    {
        Assert.True(EmptyFilter.IsEmpty);

        Assert.Null(EmptyFilter.MinId);
        Assert.Null(EmptyFilter.MaxId);
        Assert.Null(EmptyFilter.Name);
    }

    public override void Filter_ShouldBeNotEmpty()
    {
        Assert.False(Filter.IsEmpty);

        Assert.Equal(1, Filter.MinId);
        Assert.Equal(10, Filter.MaxId);
        Assert.Equal("Cigar", Filter.Name);
    }

    public override void Filter_ToString_ShouldReturnExpectedString()
    {
        const string expected = "Id=[1..10] Name=\"Cigar\"";

        string actual = Filter.ToString();

        Assert.Equal(expected, actual);
    }

    public override void Filter_CopyTo_ShouldCopyValues()
    {
        CigarFilter result = new();

        Filter.CopyTo(result);

        Assert.Equal(1, result.MinId);
        Assert.Equal(10, result.MaxId);
        Assert.Equal("Cigar", result.Name);
    }

    public override void Filter_ConstructParameterizedQuery_ShouldReturnExpectedString()
    {
        const string expected = """
                                WHERE id BETWEEN @MinId AND @MaxId AND name LIKE '%' || @Name || '%'
                                """;

        string actual = Filter.ConstructParameterizedQuery();

        Assert.Equal(expected, actual);
    }

    public override void Filter_AddParameters_ShouldAddExpectedParameters()
    {
        using NpgsqlCommand command = new NpgsqlCommand();

        Filter.AddParameters(command.Parameters);

        Assert.Equal(3, command.Parameters.Count);

        Assert.Equal("MinId", command.Parameters[0].ParameterName);
        Assert.Equal("MaxId", command.Parameters[1].ParameterName);
        Assert.Equal("Name", command.Parameters[2].ParameterName);

        Assert.Equal(command.Parameters["MinId"].Value, Filter.MinId);
        Assert.Equal(command.Parameters["MaxId"].Value, Filter.MaxId);
        Assert.Equal(command.Parameters["Name"].Value, Filter.Name);
    }

    public override void Filter_Parse_ShouldRestoreValues()
    {
        string filterString = Filter.ToString();

        IFilter<Cigar> parsed = CigarFilter.Parse(filterString);

        Assert.IsType<CigarFilter>(parsed);

        CigarFilter cigarFilter = (CigarFilter)parsed;

        Assert.Equal(1, cigarFilter.MinId);
        Assert.Equal(10, cigarFilter.MaxId);
        Assert.Equal("Cigar", cigarFilter.Name);
    }

    public override void Filter_Parse_ShouldParseCorrectly()
    {
        const string filterString = "Id=[1..10] Name=\"Cigar\"";

        IFilter<Cigar> parsed = CigarFilter.Parse(filterString);

        Assert.IsType<CigarFilter>(parsed);

        CigarFilter cigarFilter = (CigarFilter)parsed;

        Assert.Equal(1, cigarFilter.MinId);
        Assert.Equal(10, cigarFilter.MaxId);
        Assert.Equal("Cigar", cigarFilter.Name);
    }

    [Fact]
    public void Filter_AddParameters_SingleValue_ShouldAddExpectedParameters()
    {
        CigarFilter filter = new CigarFilter()
        {
            MinId = 1,
            Name = "Cigar"
        };

        using NpgsqlCommand command = new NpgsqlCommand();

        filter.AddParameters(command.Parameters);

        Assert.Equal(2, command.Parameters.Count);

        Assert.Equal("Id", command.Parameters[0].ParameterName);
        Assert.Equal("Name", command.Parameters[1].ParameterName);

        Assert.Equal(command.Parameters["Id"].Value, filter.MinId);
        Assert.Equal(command.Parameters["Name"].Value, filter.Name);
    }
}