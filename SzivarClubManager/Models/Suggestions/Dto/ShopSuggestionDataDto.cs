using System;
using System.Text.Json.Serialization;

namespace SzivarClubManager.Models.Suggestions.Dto;

[Serializable]
public class ShopSuggestionDataDto
{
    [JsonPropertyName("city")]
    public string City { get; set; }

    [JsonPropertyName("address")]
    public string Address { get; set; }

    [JsonPropertyName("openingHours")]
    public ScheduleDto[] OpeningHours { get; set; }

    public ShopSuggestionDataDto(string city, string address, ScheduleDto[] openingHours)
    {
        City = city;
        Address = address;
        OpeningHours = openingHours;
    }
}