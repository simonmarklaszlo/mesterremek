using CigiScraper.Model.Shop;
using CigiScraper.Model.Time;
using Npgsql;

namespace CigiScraper.Db;

public static class DbUploader
{
    private static readonly string ConnectionString = new NpgsqlConnectionStringBuilder()
    {
        Host = "193.201.185.129",
        Port = 5432,
        Username = "develop",
        Password = "Szivar25",
        Database = "szivarclub"
    }.ToString();

    private static readonly Dictionary<string, int> Napok = new()
    {
        { "Hétfő", 1 },
        { "Kedd", 2 },
        { "Szerda", 3 },
        { "Csütörtök", 4 },
        { "Péntek", 5 },
        { "Szombat", 6 },
        { "Vasárnap", 7 }
    };

    public static async Task Upload(Shop[] shops)
    {
        if(shops.Length == 0) return;

        await using NpgsqlConnection conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();

        Console.WriteLine("Uploading Shops");
        var ids = await UploadShops(conn, shops);
        Console.WriteLine("Uploading Schedules");
        await UploadSchedules(conn, ids, shops);
        Console.WriteLine("Upload complete");
    }

    private static async Task<int[]> UploadShops(NpgsqlConnection connection, Shop[] shops)
    {
        const string sqlBase = "INSERT INTO shops (name, address, city, created_at, updated_at, location) VALUES ";
        string[] shopInsertSqls = new string[shops.Length];

        for (var i = 0; i < shops.Length; i++)
        {
            shopInsertSqls[i] =
                $"(@name{i}, @address{i}, @city{i}, NOW(), NOW(), ST_SetSRID(ST_MakePoint(@longitude{i}, @latitude{i}), 4326))";
        }

        string fullSql = sqlBase + string.Join(',', shopInsertSqls) + "RETURNING id;";
        await using var cmd = new NpgsqlCommand(fullSql, connection);

        for (var i = 0; i < shops.Length; i++)
        {
            cmd.Parameters.AddWithValue($"@name{i}", shops[i].Name ?? "NULL");
            cmd.Parameters.AddWithValue($"@address{i}", shops[i].Address);
            cmd.Parameters.AddWithValue($"@city{i}", shops[i].City);
            cmd.Parameters.AddWithValue($"@longitude{i}", shops[i].Longitude);
            cmd.Parameters.AddWithValue($"@latitude{i}", shops[i].Latitude);
        }

        int[] ids = new int[shops.Length];
        int idx = 0;
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            ids[idx++] = reader.GetInt32(0);
        }

        return ids;
    }

    private static async Task UploadSchedules(NpgsqlConnection connection, int[] shopIds, Shop[] shops)
    {
        for (var i = 0; i < shops.Length; i++)
        {
            var shop = shops[i];
            await UploadSingleSchedule(connection, shopIds[i], shop.Schedules);
        }
    }

    private static async Task UploadSingleSchedule(NpgsqlConnection connection, int shopId, OpeningSchedule[] schedules)
    {
        var openSchedules = schedules.Where(x => !x.Closed).ToArray();
        if (openSchedules.Length == 0) return;

        const string sqlBase = "INSERT INTO shop_opening_hours (shop_id, day_id, open_hour, close_hour) VALUES ";

        string[] shopScheduleInsertSqls = new string[openSchedules.Length];
        for (var i = 0; i < openSchedules.Length; i++)
        {
            shopScheduleInsertSqls[i] = $"(@shopId, @dayId{i}, @open_hour{i}, @close_hour{i})";
        }

        string fullSql = sqlBase + string.Join(',', shopScheduleInsertSqls) + ';';

        await using var cmd = new NpgsqlCommand(fullSql, connection);

        for (var i = 0; i < openSchedules.Length; i++)
        {
            cmd.Parameters.AddWithValue("@shopId", shopId);
            cmd.Parameters.AddWithValue($"@dayId{i}", Napok[openSchedules[i].Day]);
            cmd.Parameters.AddWithValue($"@open_hour{i}", openSchedules[i].OpeningHours!.Opening);
            cmd.Parameters.AddWithValue($"@close_hour{i}", openSchedules[i].OpeningHours!.Closing);
        }

        await cmd.ExecuteNonQueryAsync();
    }
}