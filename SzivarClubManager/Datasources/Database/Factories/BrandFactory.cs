using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;

namespace SzivarClubManager.Datasources.Database.Factories;

[FactoryOf(typeof(Brand))]
public sealed class BrandFactory : IPageFactory<Brand>, IHelperFactory<Brand>
{
    private const string TableName = "cigar_brands";
    private readonly DatabaseConnection _connection;

    private Brand[]? CachedItems
    {
        get;
        set
        {
            field = value;
            CacheUpdated = DateTime.Now;
        }
    } = [];

    public DateTime CacheUpdated { get; private set; }

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
            await using var command = _connection.CreateCommand(query);
            command.Parameters.AddWithValue("name", brand.Name);
            command.Parameters.AddWithValue("id", brand.Id);
            count++;
        }

        InvalidateCache();
        return count;
    }

    public async Task<int> DeleteRange(IEnumerable<Brand> items)
    {
        var res = await CommonQueries.Delete(_connection, TableName, items.Select(x => x.Id));
        InvalidateCache();
        return res;
    }

    public Task<bool> PageExists(int page, int pageSize, IFilter<Brand> filter) => CommonQueries.PageExitstOnTable(_connection, TableName, page, pageSize, filter);

    public Task<int> GetLastPage(int pageSize, IFilter<Brand> filter) => CommonQueries.GetLastPageOnTable(_connection, TableName, pageSize, filter);

    public async Task<Brand[]> GetPage(int page, int pageSize, IFilter<Brand> filter)
    {
        string query = $"""
                        SELECT id, name 
                        FROM cigar_brands
                        {filter.ConstructParameterizedQuery()}
                        LIMIT @limit OFFSET @offset
                        """;

        await using var command = _connection.CreateCommand(query);
        filter.AddParameters(command.Parameters);
        command.Parameters.AddWithValue("offset", (page - 1) * pageSize);
        command.Parameters.AddWithValue("limit", pageSize);

        await using var reader = await command.ExecuteReaderAsync();
        List<Brand> brands = [];

        while (await reader.ReadAsync())
        {
            brands.Add(new Brand(
                reader.GetInt32(0),
                reader.GetString(1)
            ));
        }

        return brands.ToArray();
    }

    public async Task<Brand[]> GetAll()
    {
        const string query = """
                             SELECT id, name FROM cigar_brands;
                             """;

        await using var command = _connection.CreateCommand(query);

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

    public async Task<Brand[]> TryGetAllFromCache() => CachedItems ??= await GetAll();

    public async Task<Brand[]> TryGetAllFromCache(int mustContainId)
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