using System;

namespace SzivarClubManager.Models.Suggestions;

public class EditShopAddressSuggestion : Suggestion
{
    public Shop Shop { get; }
    public string NewAddress { get; }

    public EditShopAddressSuggestion(int id,
        SuggestionType type,
        SuggestionStatus status,
        User user,
        DateTime createdAt,
        DateTime updatedAt,
        SuggestionVotes votes,
        Shop shop,
        string newAddress) : base(id, type, status, user, createdAt, updatedAt, votes)
    {
        Shop = shop;
        NewAddress = newAddress;
    }
}