using System;
using System.Text.Json;
using SzivarClubManager.Models.Suggestions.Dto;
using SzivarClubManager.Models.Time;

namespace SzivarClubManager.Models.Suggestions;

public sealed class EditShopOpeningHours : Suggestion
{
    public Shop Shop { get; }
    public ShopOpeningSchedule NewOpeningHours { get; }

    public EditShopOpeningHours(int id,
        SuggestionType type,
        SuggestionStatus status,
        User user,
        DateTime createdAt,
        DateTime updatedAt,
        SuggestionVotes votes,
        Shop shop,
        string newOpeningHoursJson) : base(id, type, status, user, createdAt, updatedAt, votes)
    {
        Shop = shop;
        NewOpeningHours = ExtractData(newOpeningHoursJson);
    }

    private static ShopOpeningSchedule ExtractData(string json)
    {
        EditShopOpeningHoursDto data = JsonSerializer.Deserialize<EditShopOpeningHoursDto>(json)!;

        return ScheduleDto.ToSchedule(data.OpeningHours);
    }
}