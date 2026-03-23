using System;

namespace SzivarClubManager.Models.Suggestions;

public sealed class Suggestion : IModel
{
    public int Id { get; }
    public SuggestionType Type { get; }
    public Shop? Shop { get; }
    public string ProposedValue { get; }
    public AdditionalSuggestionData? AdditionalData { get; }
    public User User { get; }
    public SuggestionStatus Status { get; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }

    public SuggestionVotes? Votes { get; }

    public Suggestion(int id, SuggestionType type, Shop? shop, string proposedValue, AdditionalSuggestionData? additionalData, User user, SuggestionStatus status, DateTime createdAt, DateTime updatedAt, SuggestionVotes? votes)
    {
        Id = id;
        Type = type;
        Shop = shop;
        ProposedValue = proposedValue;
        AdditionalData = additionalData;
        User = user;
        Status = status;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        Votes = votes;
    }

    public string ToCopiableString() => throw new NotSupportedException();
}