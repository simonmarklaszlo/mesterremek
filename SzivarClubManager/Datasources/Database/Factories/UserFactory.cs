using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration.Factory;

namespace SzivarClubManager.Datasources.Database.Factories;

[Factory]
public sealed class UserFactory : IPageFactory<User>
{
    private const string TableName = "users";
    private readonly DatabaseConnection _connection;
    private readonly RoleFactory _roleFactory;

    public UserFactory(DatabaseConnection connection, RoleFactory roleFactory)
    {
        _connection = connection;
        _roleFactory = roleFactory;
    }

    public Task<bool> PageExists(int page, int pageSize, IFilter<User> filter) => CommonQueries.PageExitstOnTable(_connection, TableName, page, pageSize, filter);

    public Task<int> GetLastPage(int pageSize, IFilter<User> filter) => CommonQueries.GetLastPageOnTable(_connection, TableName, pageSize, filter);

    public async Task<User[]> GetPage(int page, int pageSize, IFilter<User> filter)
    {
        Dictionary<int, Role> roles = (await _roleFactory.GetAll()).ToDictionary(x => x.Id);

        string query = $"""
                        SELECT u.id, u.name, u.email, u.created_at, u.role_id
                        FROM users u 
                        {filter.ConstructParameterizedQuery()}
                        LIMIT @limit OFFSET @offset
                        """;

        await using var command = _connection.CreateCommand(query);
        command.Parameters.AddWithValue("offset", (page - 1) * pageSize);
        command.Parameters.AddWithValue("limit", pageSize);

        await using var reader = await command.ExecuteReaderAsync();
        List<User> users = [];

        while (await reader.ReadAsync())
        {
            users.Add(new User(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetDateTime(3),
                roles[reader.GetInt32(4)]
            ));
        }

        return users.ToArray();
    }

    public Task<int> AddRange(IEnumerable<User> items)
    {
        Console.WriteLine("Adding users is not supported");
        return Task.FromResult(0);
    }

    public async Task<int> EditRange(IEnumerable<User> items)
    {
        const string query = """
                             UPDATE users
                             SET name = @name,
                                 role_id = @roleId
                             WHERE id = @id;
                             """;

        int count = 0;
        foreach (var item in items)
        {
            await using var command = _connection.CreateCommand(query);
            command.Parameters.AddWithValue("name", item.Name);
            command.Parameters.AddWithValue("roleId", item.Role.Id);
            command.Parameters.AddWithValue("id", item.Id);

            count += await command.ExecuteNonQueryAsync();
        }

        return count;
    }

    public Task<int> DeleteRange(IEnumerable<User> items) => CommonQueries.Delete(_connection, TableName, items.Select(x => x.Id));

    public async Task<User?> GetModel(int id)
    {
        Dictionary<int, Role> roles = (await _roleFactory.GetAll()).ToDictionary(x => x.Id);

        string query = $"""
                        SELECT u.id, u.name, u.email, u.created_at, u.role_id
                        FROM users u 
                        WHERE u.id = @id
                        LIMIT 1
                        """;

        await using var command = _connection.CreateCommand(query);
        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync()) return null;

        return new User(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetDateTime(3),
            roles[reader.GetInt32(4)]
        );
    }

    public async Task<User[]> GetModel(IEnumerable<int> ids)
    {
        Dictionary<int, Role> roles = (await _roleFactory.GetAll()).ToDictionary(x => x.Id);

        const string query = """
                        SELECT id, name, email, created_at, role_id
                        FROM users 
                        where id = ANY(@ids)
                        """;

        await using var command = _connection.CreateCommand(query);
        command.Parameters.AddWithValue("ids", ids.ToArray());
        await using var reader = await command.ExecuteReaderAsync();
        List<User> users = [];

        while (await reader.ReadAsync())
        {
            users.Add(new User(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetDateTime(3),
                roles[reader.GetInt32(4)]
            ));
        }

        return users.ToArray();
    }
}