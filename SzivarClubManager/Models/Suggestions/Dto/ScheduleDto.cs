using System;
using System.Linq;
using System.Text.Json.Serialization;
using SzivarClubManager.Models.Time;

namespace SzivarClubManager.Models.Suggestions.Dto;

[Serializable]
public class ScheduleDto
{
    [JsonPropertyName("openHour")]
    public string OpenHour { get; set; }

    [JsonPropertyName("closeHour")]
    public string CloseHour { get; set; }

    [JsonPropertyName("dayOfWeek")]
    public string DayOfWeek { get; set; }

    public ScheduleDto(string openHour, string closeHour, string dayOfWeek)
    {
        OpenHour = openHour;
        CloseHour = closeHour;
        DayOfWeek = dayOfWeek;
    }

    public static ShopOpeningSchedule ToSchedule(ScheduleDto[] data)
    {
        var hourList = data
            .Select(x =>
            {
                Time.DayOfWeek dayOfWeek = Time.DayOfWeek.TryMatch(x.DayOfWeek) ?? Time.DayOfWeek.Monday;
                if (TimeOnly.TryParseExact(x.OpenHour, "HH:mm", out var opening) &&
                    TimeOnly.TryParseExact(x.CloseHour, "HH:mm", out var closing))
                {
                    return new ShopOpeningHour(-1, dayOfWeek, opening, closing);
                }

                return new EmptyShopOpeningHour(-1, dayOfWeek);
            })
            .ToList();

        ShopOpeningSchedule schedule = new ShopOpeningSchedule(-1, hourList);

        return schedule;
    }
}