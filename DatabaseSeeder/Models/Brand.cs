using System.Text;
using Bogus;
using Npgsql;

namespace DatabaseSeeder.Models;

public record Brand(int Id, string Name)
{
    public const int BrandCount = 50;

    public static void Clear(NpgsqlConnection connection)
    {
        const string sql = "TRUNCATE TABLE cigar_brands RESTART IDENTITY CASCADE;";
        using var command = new NpgsqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    public static void Seed(NpgsqlConnection connection)
    {
        var brands = new Faker<Brand>()
            .CustomInstantiator(f => new Brand(f.IndexFaker + 1, f.Company.CompanyName()))
            .Generate(BrandCount);

        StringBuilder querySb = new();
        querySb.Append("INSERT INTO cigar_brands (name) VALUES ");

        List<NpgsqlParameter> commandParams = [];

        foreach ((int index, var brand) in brands.Index())
        {
            querySb.Append($"(@name{index}),");
            commandParams.Add(new NpgsqlParameter($"name{index}", brand.Name));
        }

        querySb.Remove(querySb.Length - 1, 1);

        using var command = new NpgsqlCommand(querySb.ToString(), connection);
        command.Parameters.AddRange(commandParams.ToArray());

        command.ExecuteNonQuery();
    }
}