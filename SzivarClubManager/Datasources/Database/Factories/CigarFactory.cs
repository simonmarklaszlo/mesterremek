using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration.Factory;

namespace SzivarClubManager.Datasources.Database.Factories;

[Factory]
public sealed class CigarFactory : IPageFactory<Cigar>
{
    private const string TableName = "cigars";
    private readonly DatabaseConnection _connection;
    private readonly BrandFactory _brandFactory;

    public CigarFactory(DatabaseConnection connection, BrandFactory brandFactory)
    {
        _connection = connection;
        _brandFactory = brandFactory;
    }

    public Task<bool> PageExists(int page, int pageSize, IFilter<Cigar> filter) => CommonQueries.PageExitstOnTable(_connection, TableName, page, pageSize, filter);
    public Task<int> GetLastPage(int pageSize, IFilter<Cigar> filter) => CommonQueries.GetLastPageOnTable(_connection, TableName, pageSize, filter);

    public async Task<Cigar[]> GetPage(int page, int pageSize, IFilter<Cigar> filter)
    {
        Dictionary<int, Brand> brands = (await _brandFactory.GetAll()).ToDictionary(x => x.Id);

        string query = $"""
                        SELECT c.id, c.name, b.id
                        FROM cigars c 
                        JOIN cigar_brands b ON c.brand_id = b.id 
                        {filter.ConstructParameterizedQuery()}
                        LIMIT @limit OFFSET @offset
                        """;

        await using var command = _connection.CreateCommand(query);
        filter.AddParameters(command.Parameters);
        command.Parameters.AddWithValue("offset", (page - 1) * pageSize);
        command.Parameters.AddWithValue("limit", pageSize);

        await using var reader = await command.ExecuteReaderAsync();
        List<Cigar> cigars = [];

        while (await reader.ReadAsync())
        {
            cigars.Add(new Cigar(
                reader.GetInt32(0),
                reader.GetString(1),
                brands[reader.GetInt32(2)]
            ));
        }

        return cigars.ToArray();
    }

    public async Task<int> AddRange(IEnumerable<Cigar> items)
    {
        StringBuilder querySb = new();
        querySb.Append("INSERT INTO cigars (name, brand_id) VALUES ");

        var commandParams = new List<NpgsqlParameter>();

        foreach ((int index, var cigar) in items.Index())
        {
            querySb.Append($"(@name{index},@brandId{index}),");

            commandParams.Add(new NpgsqlParameter($"name{index}", cigar.Name));
            commandParams.Add(new NpgsqlParameter($"brandId{index}", cigar.Brand.Id));
        }

        if (commandParams.Count == 0) return 0;

        querySb.Remove(querySb.Length - 1, 1);

        await using var command = _connection.CreateCommand(querySb.ToString());
        command.Parameters.AddRange(commandParams.ToArray());

        return await command.ExecuteNonQueryAsync();
    }

    public async Task<int> EditRange(IEnumerable<Cigar> items)
    {
        const string query = """
                             UPDATE cigars
                             SET name = @name,
                                 brand_id = @brandId
                             WHERE id = @id;
                             """;
        int count = 0;

        foreach (var cigar in items)
        {
            await using var command = _connection.CreateCommand(query);
            command.Parameters.AddWithValue("name", cigar.Name);
            command.Parameters.AddWithValue("brandId", cigar.Brand.Id);
            command.Parameters.AddWithValue("id", cigar.Id);
            count += await command.ExecuteNonQueryAsync();
        }

        return count;
    }

    public Task<int> DeleteRange(IEnumerable<Cigar> items) => CommonQueries.Delete(_connection, TableName, items.Select(x => x.Id));
    public Task<Cigar?> GetModel(int id)
    {
        throw new System.NotImplementedException();
    }
    public Task<Cigar[]> GetModel(IEnumerable<int> ids)
    {
        throw new System.NotImplementedException();
    }
}