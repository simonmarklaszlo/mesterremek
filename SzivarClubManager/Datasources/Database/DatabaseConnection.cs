using System;
using System.Threading.Tasks;
using Npgsql;
using SzivarClubManager.Configs;

namespace SzivarClubManager.Datasources.Database;

public class DatabaseConnection
{
    public NpgsqlConnection Connection { get; }

    private DatabaseConnection(NpgsqlConnection connection)
    {
        Connection = connection;
    }

    private static readonly string ConnectionString = new NpgsqlConnectionStringBuilder
    {
        Host = GlobalConfig.Instance.DatabaseConfig.Host,
        Port = GlobalConfig.Instance.DatabaseConfig.Port,
        Username = GlobalConfig.Instance.DatabaseConfig.User,
        Password = GlobalConfig.Instance.DatabaseConfig.Password,
        Database = GlobalConfig.Instance.DatabaseConfig.DatabaseName
    }.ConnectionString;

    public static async Task<DatabaseConnection?> ConnectAsync()
    {
        var connection = new NpgsqlConnection(ConnectionString);

        try
        {
            await connection.OpenAsync();
        }
        catch (Exception)
        {
            connection.Dispose();
            return null;
        }

        return new DatabaseConnection(connection);
    }
}