using System;
using System.Text.Json;
using SzivarClubManager.Models.Suggestions.Dto;
using SzivarClubManager.Models.Time;

namespace SzivarClubManager.Models.Suggestions;

public sealed class NewShopSuggestion : Suggestion
{
    public string Name { get; }
    public string Address { get; }
    public string City { get; }
    public ShopOpeningSchedule OpeningSchedule { get; }


    public NewShopSuggestion(int id,
        SuggestionType type,
        SuggestionStatus status,
        User user,
        DateTime createdAt,
        DateTime updatedAt,
        SuggestionVotes votes,
        string name,
        string json) : base(id, type, status, user, createdAt, updatedAt, votes)
    {
        Name = name;
        (City, Address, OpeningSchedule) = ExtractData(json);
    }

    private static (string, string, ShopOpeningSchedule) ExtractData(string json)
    {
        ShopSuggestionDataDto data = JsonSerializer.Deserialize<ShopSuggestionDataDto>(json)!;

        return (data.City, data.Address, ScheduleDto.ToSchedule(data.OpeningHours));
    }
}