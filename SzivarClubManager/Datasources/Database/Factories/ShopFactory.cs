using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using Npgsql;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;

namespace SzivarClubManager.Datasources.Database.Factories;

[FactoryOf(typeof(Shop))]
public sealed class ShopFactory : IPageFactory<Shop>
{
    private const string TableName = "shops";
    private readonly DatabaseConnection _connection;

    public ShopFactory(DatabaseConnection connection)
    {
        _connection = connection;
    }

    public Task<bool> PageExists(int page, int pageSize, IFilter<Shop> filter) => CommonQueries.PageExitstOnTable(_connection, TableName, page, pageSize, filter);

    public Task<int> GetLastPage(int pageSize, IFilter<Shop> filter) => CommonQueries.GetLastPageOnTable(_connection, TableName, pageSize, filter);

    public async Task<Shop[]> GetPage(int page, int pageSize, IFilter<Shop> filter)
    {
        string query = $"""
                        SELECT id, name, address, city, created_at, updated_at, location
                        FROM shops
                        {filter.ConstructParameterizedQuery()}
                        LIMIT @limit OFFSET @offset
                        """;

        await using var command = _connection.CreateCommand(query);
        filter.AddParameters(command.Parameters);
        command.Parameters.AddWithValue("offset", (page - 1) * pageSize);
        command.Parameters.AddWithValue("limit", pageSize);

        await using var reader = await command.ExecuteReaderAsync();
        List<Shop> shops = new List<Shop>(pageSize);

        while (await reader.ReadAsync())
        {
            const string namePlaceholder = "Traffik";
            var name = reader.GetString(1);
            if (name == "NULL") name = namePlaceholder;

            shops.Add(new Shop(
                reader.GetInt32(0),
                name,
                reader.GetString(2),
                reader.GetString(3),
                reader.GetDateTime(4),
                reader.GetDateTime(5),
                CustomPgPoint.FromPgPoint(reader.GetFieldValue<Point>(6))
            ));
        }

        return shops.ToArray();
    }

    public async Task<int> AddRange(IEnumerable<Shop> items)
    {
        StringBuilder querySb = new();
        querySb.Append("INSERT INTO shops (name, address, city, location, created_at, updated_at) VALUES ");

        var commandParams = new List<NpgsqlParameter>();

        foreach ((int index, var cigar) in items.Index())
        {
            querySb.Append($"(@name{index},@address{index},@city{index},@location{index},NOW(),NOW()),");

            commandParams.Add(new NpgsqlParameter($"name{index}", cigar.Name));
            commandParams.Add(new NpgsqlParameter($"address{index}", cigar.Address));
            commandParams.Add(new NpgsqlParameter($"city{index}", cigar.City));
            commandParams.Add(new NpgsqlParameter($"location{index}", cigar.Location.ToPgPoint()));
        }

        if (commandParams.Count == 0) return 0;

        querySb.Remove(querySb.Length - 1, 1);

        await using var command = _connection.CreateCommand(querySb.ToString());
        command.Parameters.AddRange(commandParams.ToArray());

        return await command.ExecuteNonQueryAsync();
    }

    public async Task<int> EditRange(IEnumerable<Shop> items)
    {
        const string query = """
                             UPDATE shops
                             SET name = @name,
                                 address = @address,
                                 city = @city,
                                 location = @location,
                                 updated_at = NOW()
                             WHERE id = @id;
                             """;
        int count = 0;

        foreach (var item in items)
        {
            await using var command = _connection.CreateCommand(query);
            command.Parameters.AddWithValue("name", item.Name);
            command.Parameters.AddWithValue("address", item.Address);
            command.Parameters.AddWithValue("city", item.City);
            command.Parameters.AddWithValue("location", item.Location.ToPgPoint());
            command.Parameters.AddWithValue("id", item.Id);
            count += await command.ExecuteNonQueryAsync();
        }

        return count;
    }

    public Task<int> DeleteRange(IEnumerable<Shop> items) => CommonQueries.Delete(_connection, TableName, items.Select(x => x.Id));
}