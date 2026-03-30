using System;
using System.Text.Json.Serialization;

namespace SzivarClubManager.Models.Suggestions.Dto;

[Serializable]
public class EditShopOpeningHoursDto
{
    [JsonPropertyName("openingHours")]
    public ScheduleDto[] OpeningHours { get; set; }

    public EditShopOpeningHoursDto(ScheduleDto[] openingHours)
    {
        OpeningHours = openingHours;
    }
}