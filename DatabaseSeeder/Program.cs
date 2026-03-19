using DatabaseSeeder.Models;
using DotNetEnv;
using Npgsql;


if (!File.Exists(".env"))
{
    Console.WriteLine(".env file required!");
    Environment.Exit(-1);
}

NpgsqlConnection.GlobalTypeMapper.UseNetTopologySuite();
Env.Load();

NpgsqlConnection connection = new NpgsqlConnection(new NpgsqlConnectionStringBuilder
{
    Host = Env.GetString("DB_HOST"),
    Port = Env.GetInt("DB_PORT", 5432),
    Username = Env.GetString("DB_USER"),
    Password = Env.GetString("DB_PASSWORD"),
    Database = Env.GetString("DB_DATABASE")
}.ConnectionString
);

try
{
    await connection.OpenAsync();
}
catch (Exception e)
{
    Console.WriteLine("Failed to connect to database: " + e.Message);
    Environment.Exit(-1);
}

Shop.Clear(connection);
Cigar.Clear(connection);
Brand.Clear(connection);

Brand.Seed(connection);
Cigar.Seed(connection);
Shop.Seed(connection);