using System.Collections.Generic;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using Npgsql;
using SzivarClubManager.Datasources.Pagination;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;

namespace SzivarClubManager.Datasources.Database.Factories;

[FactoryOf(typeof(Shop), true)]
public class ShopFactory : IPageFactory<Shop>
{
    private const string TableName = "shops";
    private readonly DatabaseConnection _connection;

    public ShopFactory(DatabaseConnection connection)
    {
        _connection = connection;
    }

    public Task<bool> PageExists(int page, int pageSize) => _connection.PageExitstOnTable(TableName, page, pageSize);

    public Task<int> GetLastPage(int pageSize) => _connection.GetLastPageOnTable(TableName, pageSize);

    public async Task<Shop[]> GetPage(int page, int pageSize)
    {
        const string query = """
                             SELECT id, name, address, city, created_at, updated_at, location
                             FROM shops
                             LIMIT @limit OFFSET @offset
                             """;

        await using var command = new NpgsqlCommand(query, _connection.Connection);
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
}