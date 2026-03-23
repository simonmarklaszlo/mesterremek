using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SzivarClubManager.Models.Time;
using DayOfWeek = SzivarClubManager.Models.Time.DayOfWeek;

namespace SzivarClubManager.Models.Suggestions;

[Serializable]
public sealed class AdditionalSuggestionData
{
    [JsonPropertyName("city")]
    public string? City { get; }

    [JsonPropertyName("address")]
    public string? Address { get; }

    [JsonPropertyName("openingHours")]
    public OpeningHourDto[]? OpeningHours { get; }

    [JsonIgnore]
    public ShopOpeningSchedule? ShopOpeningSchedule => OpeningHours == null ? null : field ??= ParseSchedule();

    [JsonConstructor]
    public AdditionalSuggestionData(string? city, string? address, OpeningHourDto[]? openingHours)
    {
        City = city;
        Address = address;
        OpeningHours = openingHours;
    }

    private ShopOpeningSchedule ParseSchedule()
    {
        if (OpeningHours is null) throw new ArgumentNullException(nameof(OpeningHours));

        List<ShopOpeningHour> openingHours = new(7);

        foreach ((string openHour, string closeHour, string dayOfWeek) in OpeningHours)
        {
            var day = DayOfWeek.TryMatch(dayOfWeek) ?? DayOfWeek.Monday;
            if (TimeOnly.TryParse(openHour, out var openingDateTime) && TimeOnly.TryParse(closeHour, out var closingDateTime))
            {
                openingHours.Add(new ShopOpeningHour(-1, day, openingDateTime, closingDateTime));
            }

            openingHours.Add(new EmptyShopOpeningHour(-1, day));
        }

        return new ShopOpeningSchedule(-1, openingHours);
    }

    public static AdditionalSuggestionData Empty => new(null, null, null);
}

public class OpeningHourDto
{
    public string OpenHour { get; set; } = null!;
    public string CloseHour { get; set; } = null!;
    public string DayOfWeek { get; set; } = null!;

    public void Deconstruct(out string openHour, out string closeHour, out string dayOfWeek) => (openHour, closeHour, dayOfWeek) = (OpenHour, CloseHour, DayOfWeek);
}