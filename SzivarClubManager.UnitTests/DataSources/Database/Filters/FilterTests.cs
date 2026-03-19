using Npgsql;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Models;

namespace SzivarClubManager.UnitTests.DataSources.Database.Filters;

public abstract class FilterTests<TFilter, TModel>
    where TFilter : IFilter<TModel>, new()
    where TModel : class, IModel
{
    protected readonly TFilter EmptyFilter;

    protected FilterTests()
    {
        EmptyFilter = new TFilter();
    }

    [Fact]
    public void NewFilter_IsEmpty_ShouldReturnTrue()
    {
        Assert.True(EmptyFilter.IsEmpty);
    }

    [Fact]
    public void NewFilter_ToString_ShouldReturnEmptyString()
    {
        const string expected = "";

        string actual = EmptyFilter.ToString();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void NewFilter_ConstructParameterizedQuery_ShouldReturnEmptyString()
    {
        const string expected = "";

        string actual = EmptyFilter.ConstructParameterizedQuery();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void NewFilter_AddParameters_ShouldNotAddAny()
    {
        using NpgsqlCommand command = new NpgsqlCommand();

        EmptyFilter.AddParameters(command.Parameters);

        Assert.Empty(command.Parameters);
    }

    [Fact]
    public void NewFilter_Parse_ShouldReturnEmptyFilter()
    {
        IFilter<TModel> filter = TFilter.Parse(string.Empty);

        Assert.True(filter.IsEmpty);
    }

    [Fact]
    public abstract void EmptyFilter_ShouldBeEmpty();

    [Fact]
    public abstract void Filter_ShouldBeNotEmpty();

    [Fact]
    public abstract void Filter_ToString_ShouldReturnExpectedString();

    [Fact]
    public abstract void Filter_CopyTo_ShouldCopyValues();

    [Fact]
    public abstract void Filter_ConstructParameterizedQuery_ShouldReturnExpectedString();

    [Fact]
    public abstract void Filter_AddParameters_ShouldAddExpectedParameters();

    [Fact]
    public abstract void Filter_Parse_ShouldRestoreValues();

    [Fact]
    public abstract void Filter_Parse_ShouldParseCorrectly();
}