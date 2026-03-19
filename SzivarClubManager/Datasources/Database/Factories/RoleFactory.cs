using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;

namespace SzivarClubManager.Datasources.Database.Factories;

[FactoryOf(typeof(Role))]
public sealed class RoleFactory : IHelperFactory<Role>
{
    private const string TableName = "roles";
    private readonly DatabaseConnection _connection;

    private Role[]? CachedItems
    {
        get;
        set
        {
            field = value;
            CacheUpdated = DateTime.Now;
        }
    } = [];

    public DateTime CacheUpdated { get; private set; }

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

        if (commandParams.Count == 0)
        {
            InvalidateCache();
            return 0;
        }

        querySb.Remove(querySb.Length - 1, 1);

        await using var command = _connection.CreateCommand(querySb.ToString());
        command.Parameters.AddRange(commandParams.ToArray());

        var res = await command.ExecuteNonQueryAsync();
        InvalidateCache();
        return res;
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
            await using var command = _connection.CreateCommand(query);
            command.Parameters.AddWithValue("name", item.Name);
            command.Parameters.AddWithValue("id", item.Id);
            count += await command.ExecuteNonQueryAsync();
        }

        InvalidateCache();
        return count;
    }

    public async Task<int> DeleteRange(IEnumerable<Role> items)
    {
        var res = await CommonQueries.Delete(_connection, TableName, items.Select(x => x.Id));
        InvalidateCache();
        return res;
    }

    public async Task<Role[]> GetAll()
    {
        const string query = "SELECT id, name FROM roles;";

        await using var command = _connection.CreateCommand(query);

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

    public async Task<Role[]> TryGetAllFromCache() => CachedItems ??= await GetAll();

    public async Task<Role[]> TryGetAllFromCache(int mustContainId)
    {
        if (CachedItems is null)
        {
            return CachedItems = await GetAll();
        }

        if (CachedItems.Any(x => x.Id == mustContainId))
        {
            return CachedItems;
        }

        return CachedItems = await GetAll();
    }

    public void InvalidateCache() => CachedItems = null;
}