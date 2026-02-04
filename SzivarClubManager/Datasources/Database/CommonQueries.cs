using System;
using System.Threading.Tasks;
using Npgsql;

namespace SzivarClubManager.Datasources.Database;

public static class CommonQueries
{
    public static async Task<bool> PageExitstOnTable(this DatabaseConnection connection, string table, int page, int pageSize)
    {
        string query = $"SELECT EXISTS ( SELECT 1 FROM {table} LIMIT @limit OFFSET @offset )";

        await using var command = new NpgsqlCommand(query, connection.Connection);
        command.Parameters.AddWithValue("table", table);
        command.Parameters.AddWithValue("offset", (page - 1) * pageSize);
        command.Parameters.AddWithValue("limit", pageSize);

        return (bool)(await command.ExecuteScalarAsync() ?? Task.FromResult(false));
    }

    public static async Task<int> GetLastPageOnTable(this DatabaseConnection connection, string table, int pageSize)
    {
        string query = $"SELECT COUNT(*) FROM {table}";

        await using var command = new NpgsqlCommand(query, connection.Connection);

        int count = Convert.ToInt32(await command.ExecuteScalarAsync());

        if (count < pageSize)
        {
            return 1;
        }

        return (int)Math.Ceiling((float)count / pageSize);
    }
}