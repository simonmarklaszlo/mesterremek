using Npgsql;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Models;

namespace SzivarClubManager.UnitTests.DataSources.Database.Filters;

public class UserFilterTests : FilterTests<UserFilter, User>
{
    private readonly UserFilter Filter;

    public UserFilterTests()
    {
        Filter = new UserFilter
        {
            MinId = 1,
            MaxId = 10,
            Email = "UserEmail",
            Name = "User"
        };
    }

    public override void EmptyFilter_ShouldBeEmpty()
    {
        Assert.True(EmptyFilter.IsEmpty);

        Assert.Null(EmptyFilter.MinId);
        Assert.Null(EmptyFilter.MaxId);
        Assert.Null(EmptyFilter.Email);
        Assert.Null(EmptyFilter.Name);
    }

    public override void Filter_ShouldBeNotEmpty()
    {
        Assert.False(Filter.IsEmpty);

        Assert.Equal(1, Filter.MinId);
        Assert.Equal(10, Filter.MaxId);
        Assert.Equal("UserEmail", Filter.Email);
        Assert.Equal("User", Filter.Name);
    }

    public override void Filter_ToString_ShouldReturnExpectedString()
    {
        const string expected = "Id=[1..10] Email=\"UserEmail\" Name=\"User\"";

        string actual = Filter.ToString();

        Assert.Equal(expected, actual);
    }

    public override void Filter_CopyTo_ShouldCopyValues()
    {
        UserFilter result = new();

        Filter.CopyTo(result);

        Assert.Equal(1, result.MinId);
        Assert.Equal(10, result.MaxId);
        Assert.Equal("UserEmail", result.Email);
        Assert.Equal("User", result.Name);
    }

    public override void Filter_ConstructParameterizedQuery_ShouldReturnExpectedString()
    {
        const string expected = """
                                WHERE id BETWEEN @MinId AND @MaxId AND email LIKE '%' || @Email || '%' AND name LIKE '%' || @Name || '%'
                                """;

        string actual = Filter.ConstructParameterizedQuery();

        Assert.Equal(expected, actual);
    }

    public override void Filter_AddParameters_ShouldAddExpectedParameters()
    {
        using NpgsqlCommand command = new NpgsqlCommand();

        Filter.AddParameters(command.Parameters);

        Assert.Equal(4, command.Parameters.Count);

        Assert.Equal("MinId", command.Parameters[0].ParameterName);
        Assert.Equal("MaxId", command.Parameters[1].ParameterName);
        Assert.Equal("Email", command.Parameters[2].ParameterName);
        Assert.Equal("Name", command.Parameters[3].ParameterName);

        Assert.Equal(command.Parameters["MinId"].Value, Filter.MinId);
        Assert.Equal(command.Parameters["MaxId"].Value, Filter.MaxId);
        Assert.Equal(command.Parameters["Email"].Value, Filter.Email);
        Assert.Equal(command.Parameters["Name"].Value, Filter.Name);
    }

    public override void Filter_Parse_ShouldRestoreValues()
    {
        string filterString = Filter.ToString();

        IFilter<User> parsed = UserFilter.Parse(filterString);

        Assert.IsType<UserFilter>(parsed);

        UserFilter userFilter = (UserFilter)parsed;

        Assert.Equal(1, userFilter.MinId);
        Assert.Equal(10, userFilter.MaxId);
        Assert.Equal("UserEmail", userFilter.Email);
        Assert.Equal("User", userFilter.Name);
    }

    public override void Filter_Parse_ShouldParseCorrectly()
    {
        const string filterString = "Id=[1..10] Email=\"UserEmail\" Name=\"User\"";

        IFilter<User> parsed = UserFilter.Parse(filterString);

        Assert.IsType<UserFilter>(parsed);

        UserFilter userFilter = (UserFilter)parsed;

        Assert.Equal(1, userFilter.MinId);
        Assert.Equal(10, userFilter.MaxId);
        Assert.Equal("UserEmail", userFilter.Email);
        Assert.Equal("User", userFilter.Name);
    }

    [Fact]
    public void Filter_AddParameters_SingleValue_ShouldAddExpectedParameters()
    {
        UserFilter filter = new UserFilter()
        {
            MinId = 1,
            Email = "UserEmail",
            Name = "User"
        };

        using NpgsqlCommand command = new NpgsqlCommand();

        filter.AddParameters(command.Parameters);

        Assert.Equal(3, command.Parameters.Count);

        Assert.Equal("Id", command.Parameters[0].ParameterName);
        Assert.Equal("Email", command.Parameters[1].ParameterName);
        Assert.Equal("Name", command.Parameters[2].ParameterName);

        Assert.Equal(command.Parameters["Id"].Value, filter.MinId);
        Assert.Equal(command.Parameters["Email"].Value, filter.Email);
        Assert.Equal(command.Parameters["Name"].Value, filter.Name);
    }
}