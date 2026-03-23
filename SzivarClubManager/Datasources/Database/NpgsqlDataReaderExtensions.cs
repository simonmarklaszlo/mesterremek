using Npgsql;

namespace SzivarClubManager.Datasources.Database;

public static class NpgsqlDataReaderExtensions
{
    extension(NpgsqlDataReader reader)
    {
        public int? GetNullableInt32(int ordinal) => reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
        public string? GetNullableString(int ordinal) => reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }
}