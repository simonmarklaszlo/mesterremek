using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;

namespace SzivarClubManager.Datasources.Database.Factories;

[FactoryOf(typeof(Role), true)]
public sealed class RoleFactory : IHelperFactory<Role>
{
    private const string TableName = "roles";
    private readonly DatabaseConnection _connection;

    private Role[]? _cachedItems = [];

    public RoleFactory(DatabaseConnection connection)
    {
        _connection = connection;
    }


    public async Task<int> AddRange(IEnumerable<Role> items)
    {
        StringBuilder querySb = new();
        querySb.Append("INSERT INTO roles (name) VALUES ");

        List<NpgsqlParameter> commandParams = [];

        foreach ((int index, var role) in items.Index())
        {
            querySb.Append($"(@name{index}),");
            commandParams.Add(new NpgsqlParameter($"name{index}", role.Name));
        }

        querySb.Remove(querySb.Length - 1, 1);

        await using var command = new NpgsqlCommand(querySb.ToString(), _connection.Connection);
        command.Parameters.AddRange(commandParams.ToArray());

        return await command.ExecuteNonQueryAsync();
    }

    public async Task<int> EditRange(IEnumerable<Role> items)
    {
        const string query = """
                             UPDATE roles
                             SET name = @name
                             WHERE id = @id;
                             """;

        int count = 0;

        foreach (var item in items)
        {
            await using var command = new NpgsqlCommand(query, _connection.Connection);
            command.Parameters.AddWithValue("name", item.Name);
            command.Parameters.AddWithValue("id", item.Id);
            count += command.ExecuteNonQuery();
        }

        return count;
    }

    public Task<int> DeleteRange(IEnumerable<Role> items) => CommonQueries.Delete(_connection, TableName, items.Select(x => x.Id));

    public async Task<Role[]> GetAll()
    {
        const string query = "SELECT id, name FROM roles;";

        await using var command = new NpgsqlCommand(query, _connection.Connection);

        List<Role> roles = [];
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            roles.Add(new Role(
                reader.GetInt32(0),
                reader.GetString(1)
            ));
        }

        return roles.ToArray();
    }

    public async Task<Role[]> TryGetAllFromCache() => _cachedItems ??= await GetAll();

    public async Task<Role[]> TryGetAllFromCache(int mustContainId)
    {
        if (_cachedItems is null)
        {
            return _cachedItems = await GetAll();
        }

        if (_cachedItems.Any(x => x.Id == mustContainId))
        {
            return _cachedItems;
        }

        return _cachedItems = await GetAll();
    }
}