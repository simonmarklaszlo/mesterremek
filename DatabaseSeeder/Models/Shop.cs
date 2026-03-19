using System.Text;
using Bogus;
using NetTopologySuite.Geometries;
using Npgsql;

namespace DatabaseSeeder.Models;

public record Shop(int Id, string Name, string Address, string City, DateTime CreatedAt, DateTime UpdatedAt, CustomPgPoint Location)
{
    public const int ShopCount = 200;

    public static void Clear(NpgsqlConnection connection)
    {
        const string sql = "TRUNCATE TABLE shops RESTART IDENTITY CASCADE;";
        using var command = new NpgsqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    public static void Seed(NpgsqlConnection connection)
    {
        var shops = new Faker<Shop>()
             .CustomInstantiator(f =>
             {
                 var created = f.Date.Past(3);

                 return new Shop(
                     f.IndexFaker,
                     f.Company.CompanyName(),
                     f.Address.StreetAddress(),
                     f.Address.City(),
                     created,
                     f.Date.Between(created, DateTime.Now),
                     new CustomPgPoint(
                         f.Address.Longitude(),
                         f.Address.Latitude()
                     )
                 );
             })
             .Generate(ShopCount);

        StringBuilder querySb = new();
        querySb.Append("INSERT INTO shops (name, address, city, location, created_at, updated_at) VALUES ");

        var commandParams = new List<NpgsqlParameter>();

        foreach ((int index, var cigar) in shops.Index())
        {
            querySb.Append($"(@name{index},@address{index},@city{index},@location{index},@created_at{index},@updated_at{index}),");

            commandParams.Add(new NpgsqlParameter($"name{index}", cigar.Name));
            commandParams.Add(new NpgsqlParameter($"address{index}", cigar.Address));
            commandParams.Add(new NpgsqlParameter($"city{index}", cigar.City));
            commandParams.Add(new NpgsqlParameter($"location{index}", cigar.Location.ToPgPoint()));
            commandParams.Add(new NpgsqlParameter($"created_at{index}", cigar.CreatedAt));
            commandParams.Add(new NpgsqlParameter($"updated_at{index}", cigar.UpdatedAt));
        }

        querySb.Remove(querySb.Length - 1, 1);

        using var command = new NpgsqlCommand(querySb.ToString(), connection);
        command.Parameters.AddRange(commandParams.ToArray());

        command.ExecuteNonQuery();
    }
}

public record CustomPgPoint(double Lon, double Lat)
{
    private const int Srid = 4326;
    public Point ToPgPoint() => new(Lon, Lat) { SRID = Srid };
}