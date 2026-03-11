using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SzivarClubManager.Datasources.Database;

public static class CommonQueries
{
    public static async Task<bool> PageExitstOnTable(DatabaseConnection connection, string table, int page, int pageSize)
    {
        string query = $"SELECT EXISTS ( SELECT 1 FROM {table} LIMIT @limit OFFSET @offset )";

        await using var command = connection.CreateCommand(query);
        command.Parameters.AddWithValue("table", table);
        command.Parameters.AddWithValue("offset", (page - 1) * pageSize);
        command.Parameters.AddWithValue("limit", pageSize);

        return (bool)(await command.ExecuteScalarAsync() ?? Task.FromResult(false));
    }

    public static async Task<int> GetLastPageOnTable(DatabaseConnection connection, string table, int pageSize)
    {
        string query = $"SELECT COUNT(*) FROM {table}";

        await using var command = connection.CreateCommand(query);

        int count = Convert.ToInt32(await command.ExecuteScalarAsync());

        if (count < pageSize)
        {
            return 1;
        }

        return (int)Math.Ceiling((float)count / pageSize);
    }

    public static async Task<bool> Delete(DatabaseConnection connection, string table, int id)
    {
        string query = $"""
                        DELETE FROM {table}
                        WHERE id = @id;
                        """;

        await using var command = connection.CreateCommand(query);
        command.Parameters.AddWithValue("id", id);

        return await command.ExecuteNonQueryAsync() == 1;
    }

    public static async Task<int> Delete(DatabaseConnection connection, string table, IEnumerable<int> ids)
    {
        string query = $"""
                        DELETE FROM {table}
                        WHERE id = ANY(@ids);
                        """;

        int[] idArr = ids as int[] ?? ids.ToArray();

        await using var command = connection.CreateCommand(query);
        command.Parameters.AddWithValue("ids", idArr);

        return await command.ExecuteNonQueryAsync();
    }
}