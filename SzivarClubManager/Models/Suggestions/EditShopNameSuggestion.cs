using System;

namespace SzivarClubManager.Models.Suggestions;

public sealed class EditShopNameSuggestion : Suggestion
{
    public Shop Shop { get; }
    public string NewName { get; }


    public EditShopNameSuggestion(int id,
        SuggestionType type,
        SuggestionStatus status,
        User user,
        DateTime createdAt,
        DateTime updatedAt,
        SuggestionVotes votes,
        Shop shop,
        string newName) : base(id, type, status, user, createdAt, updatedAt, votes)
    {
        Shop = shop;
        NewName = newName;
    }
}