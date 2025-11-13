using Npgsql;

namespace CigiScraper.Db;

public static class DbInfo
{
    private static readonly string ConnectionString = new NpgsqlConnectionStringBuilder()
    {
        Host = "193.201.185.129",
        Port = 5432,
        Username = "develop",
        Password = "Szivar25",
        Database = "szivarclub"
    }.ToString();

    private static readonly string[] IgnoredTables = ["spatial_ref_sys", "geography_columns", "geometry_columns"];

    public static async Task<DbColumn[]> GetInfo()
    {
        List<DbColumn> columns = [];

        foreach (string table in await GetTableNames())
        {
            foreach ((string columnName, string dataType) in await GetColumnNames(table))
            {
                columns.Add(new DbColumn(table, columnName, dataType));
            }
        }

        return columns.ToArray();
    }

    public static async Task<string[]> GetTableNames()
    {
        await using NpgsqlConnection conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        const string query = """
                             SELECT table_name
                             FROM information_schema.tables
                             WHERE table_schema = 'public'
                             ORDER BY table_name;
                             """;

        await using var cmd = new NpgsqlCommand(query, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        var tableNames = new List<string>();

        while (await reader.ReadAsync())
        {
            tableNames.Add(reader.GetString(0));
        }

        return tableNames.Where(x => !IgnoredTables.Contains(x)).ToArray();
    }

    public static async Task<(string ColumnName, string DataType)[]> GetColumnNames(string tableName)
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();

        const string query = """
                             SELECT column_name, data_type
                             FROM information_schema.columns
                             WHERE table_schema = 'public' AND table_name = @tableName
                             ORDER BY ordinal_position;
                             """;

        await using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@tableName", tableName);

        var columns = new List<(string, string)>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            string columnName = reader.GetString(0);
            string dataType = reader.GetString(1);
            columns.Add((columnName, dataType));
        }

        return columns.ToArray();
    }
}