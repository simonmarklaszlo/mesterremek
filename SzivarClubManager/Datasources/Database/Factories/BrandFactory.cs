using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;

namespace SzivarClubManager.Datasources.Database.Factories;

[FactoryOf(typeof(Brand), true)]
public sealed class BrandFactory : IHelperFactory<Brand>
{
    private const string TableName = "cigar_brands";
    private readonly DatabaseConnection _connection;

    private Brand[]? _cachedItems = [];

    public BrandFactory(DatabaseConnection connection)
    {
        _connection = connection;
    }


    public async Task<int> AddRange(IEnumerable<Brand> items)
    {
        StringBuilder querySb = new();
        querySb.Append("INSERT INTO cigar_brands (name) VALUES ");

        List<NpgsqlParameter> commandParams = [];

        foreach ((int index, var brand) in items.Index())
        {
            querySb.Append($"(@name{index}),");
            commandParams.Add(new NpgsqlParameter($"name{index}", brand.Name));
        }

        querySb.Remove(querySb.Length - 1, 1);

        await using var command = new NpgsqlCommand(querySb.ToString(), _connection.Connection);
        command.Parameters.AddRange(commandParams.ToArray());

        return command.ExecuteNonQuery();
    }

    public async Task<int> EditRange(IEnumerable<Brand> items)
    {
        const string query = """
                             UPDATE cigar_brands
                             SET name = @name
                             WHERE id = @id;
                             """;
        int count = 0;

        foreach (var brand in items)
        {
            await using var command = new NpgsqlCommand(query, _connection.Connection);
            command.Parameters.AddWithValue("name", brand.Name);
            command.Parameters.AddWithValue("id", brand.Id);
            count++;
        }

        return count;
    }

    public Task<int> DeleteRange(IEnumerable<Brand> items) => CommonQueries.Delete(_connection, TableName, items.Select(x => x.Id));

    public async Task<Brand[]> GetAll()
    {
        const string query = """
                             SELECT id, name FROM cigar_brands;
                             """;

        await using var command = new NpgsqlCommand(query, _connection.Connection);

        List<Brand> brands = [];
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            brands.Add(new Brand(
                reader.GetInt32(0),
                reader.GetString(1)
            ));
        }

        return brands.ToArray();
    }

    public async Task<Brand[]> TryGetAllFromCache() => _cachedItems ??= await GetAll();

    public async Task<Brand[]> TryGetAllFromCache(int mustContainId)
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