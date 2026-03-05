using System.Text;
using Bogus;
using Npgsql;

namespace DatabaseSeeder.Models;

public record Cigar(int Id, string Name, int BrandId)
{
    public const int CigarCount = 200;

    public static void Clear(NpgsqlConnection connection)
    {
        const string sql = "TRUNCATE TABLE cigars RESTART IDENTITY CASCADE;";
        using var command = new NpgsqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    public static void Seed(NpgsqlConnection connection)
    {
        var brands = new Faker<Cigar>()
            .CustomInstantiator(f => new Cigar(f.IndexFaker + 1, f.Commerce.ProductName(), Random.Shared.Next(1,Brand.BrandCount)))
            .Generate(CigarCount);

        StringBuilder querySb = new();
        querySb.Append("INSERT INTO cigars (id, name, brand_id) VALUES ");

        List<NpgsqlParameter> commandParams = [];

        foreach ((int index, var brand) in brands.Index())
        {
            querySb.Append($"(@id{index}, @name{index}, @brandId{index}),");
            commandParams.Add(new NpgsqlParameter($"id{index}", brand.Id));
            commandParams.Add(new NpgsqlParameter($"name{index}", brand.Name));
            commandParams.Add(new NpgsqlParameter($"brandId{index}", brand.BrandId));
        }

        querySb.Remove(querySb.Length - 1, 1);

        using var command = new NpgsqlCommand(querySb.ToString(), connection);
        command.Parameters.AddRange(commandParams.ToArray());

        command.ExecuteNonQuery();
    }
}