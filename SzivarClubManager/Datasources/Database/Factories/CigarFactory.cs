using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;

namespace SzivarClubManager.Datasources.Database.Factories;

[FactoryOf(typeof(Cigar), true)]
public class CigarFactory : IPageFactory<Cigar>
{
    private const string TableName = "cigars";
    private readonly DatabaseConnection _connection;

    public CigarFactory(DatabaseConnection connection)
    {
        _connection = connection;
    }

    public Task<bool> PageExists(int page, int pageSize) => _connection.PageExitstOnTable(TableName, page, pageSize);
    public Task<int> GetLastPage(int pageSize) => _connection.GetLastPageOnTable(TableName, pageSize);

    public async Task<Cigar[]> GetPage(int page, int pageSize)
    {
        const string query = """
                             SELECT c.id, c.name, b.id, b.name
                             FROM cigars c 
                             JOIN roles b ON c.brand_id = b.id 
                             LIMIT @limit OFFSET @offset
                             """;

        await using var command = new NpgsqlCommand(query, _connection.Connection);
        command.Parameters.AddWithValue("offset", (page - 1) * pageSize);
        command.Parameters.AddWithValue("limit", pageSize);

        await using var reader = await command.ExecuteReaderAsync();
        Dictionary<int, Brand> brands = [];
        List<Cigar> cigars = [];

        while (await reader.ReadAsync())
        {
            int cigarId = reader.GetInt32(0);
            string cigarName = reader.GetString(1);
            int brandId = reader.GetInt32(2);
            string brandName = reader.GetString(3);

            if (!brands.TryGetValue(brandId, out Brand? value))
            {
                value = new Brand(brandId, brandName);
                brands.Add(brandId, value);
            }

            cigars.Add(new Cigar(cigarId, cigarName, value));
        }

        return cigars.ToArray();
    }
}