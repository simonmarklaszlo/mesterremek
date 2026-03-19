using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Models;

namespace SzivarClubManager.Datasources.Database;

public static class CommonQueries
{
    public static async Task<bool> PageExitstOnTable<T>(DatabaseConnection connection, string table, int page, int pageSize, IFilter<T> filter) where T : class, IModel
    {
        string query = $"""
                        SELECT EXISTS (
                            SELECT 1
                            FROM {table}
                            {filter.ConstructParameterizedQuery()}
                            LIMIT @limit OFFSET @offset
                        );
                        """;

        await using var command = connection.CreateCommand(query);
        command.Parameters.AddWithValue("table", table);
        filter.AddParameters(command.Parameters);
        command.Parameters.AddWithValue("offset", (page - 1) * pageSize);
        command.Parameters.AddWithValue("limit", pageSize);

        return (bool)(await command.ExecuteScalarAsync() ?? Task.FromResult(false));
    }

    public static async Task<int> GetLastPageOnTable<T>(DatabaseConnection connection, string table, int pageSize, IFilter<T> filter) where T : class, IModel
    {
        string query = $"""
                        SELECT COUNT(*) 
                        FROM {table}
                        {filter.ConstructParameterizedQuery()}
                        """;

        await using var command = connection.CreateCommand(query);
        filter.AddParameters(command.Parameters);

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

        if (idArr.Length == 0) return 0;

        await using var command = connection.CreateCommand(query);
        command.Parameters.AddWithValue("ids", idArr);

        return await command.ExecuteNonQueryAsync();
    }
}