using System;
using System.Threading.Tasks;
using Npgsql;
using SzivarClubManager.Configs;

namespace SzivarClubManager.Datasources.Database;

public sealed class DatabaseConnection
{
    private readonly NpgsqlConnection _connection;

    private DatabaseConnection(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    public DatabaseCommandContext CreateCommand(string query) => new(query, _connection);

    public static async Task<DatabaseConnection?> ConnectAsync()
    {
        var builder = new NpgsqlDataSourceBuilder(new NpgsqlConnectionStringBuilder
        {
            Host = GlobalConfig.Instance.DatabaseConfig.Host,
            Port = GlobalConfig.Instance.DatabaseConfig.Port,
            Username = GlobalConfig.Instance.DatabaseConfig.User,
            Password = GlobalConfig.Instance.DatabaseConfig.Password,
            Database = GlobalConfig.Instance.DatabaseConfig.DatabaseName
        }.ConnectionString);
        builder.UseNetTopologySuite();

        await using var dataSource = builder.Build();

        try
        {
            var connection = await dataSource.OpenConnectionAsync();
            return new DatabaseConnection(connection);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
}