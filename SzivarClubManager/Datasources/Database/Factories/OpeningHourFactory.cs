using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models;
using SzivarClubManager.Models.Time;
using SzivarClubManager.SourceGeneration;
using DayOfWeek = SzivarClubManager.Models.Time.DayOfWeek;

namespace SzivarClubManager.Datasources.Database.Factories;

[FactoryOf(typeof(ShopOpeningHour))]
public class OpeningHourFactory : IFactory
{
    private readonly DatabaseConnection _connection;

    public OpeningHourFactory(DatabaseConnection connection)
    {
        _connection = connection;
    }

    public async Task<ShopOpeningSchedule> GetSchedule(Shop shop)
    {
        const string query = """
                             SELECT id, day_id, open_hour, close_hour 
                             FROM shop_opening_hours
                             WHERE shop_id = @shopId;
                             """;

        List<ShopOpeningHour> openingHours = new(7);

        await using var command = _connection.CreateCommand(query);
        command.Parameters.AddWithValue("shopId", shop.Id);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            openingHours.Add(new ShopOpeningHour(
                reader.GetInt32(0),
                DayOfWeek.FromId(reader.GetInt32(1)),
                reader.GetFieldValue<TimeOnly>(2),
                reader.GetFieldValue<TimeOnly>(3)
            ));
        }

        return new ShopOpeningSchedule(shop.Id, openingHours);
    }
}