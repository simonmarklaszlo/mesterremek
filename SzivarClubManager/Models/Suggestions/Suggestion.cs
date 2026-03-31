using System;
using System.Threading.Tasks;
using SzivarClubManager.Datasources.Database.Factories;
using SzivarClubManager.Datasources.Factory;

namespace SzivarClubManager.Models.Suggestions;

public abstract class Suggestion : IModel
{
    public int Id { get; }
    public SuggestionType Type { get; }
    public SuggestionStatus Status { get; }
    public User User { get; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }
    public SuggestionVotes Votes { get; }

    public string CreatedAtString => field ??= CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
    public string UpdatedAtString => field ??= UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss");

    public bool CanBeApproved => Status.Id is not 2 and not 4;

    protected Suggestion(int id, SuggestionType type, SuggestionStatus status, User user, DateTime createdAt, DateTime updatedAt, SuggestionVotes votes)
    {
        Id = id;
        Type = type;
        Status = status;
        User = user;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        Votes = votes;
    }

    public string ToCopiableString() => throw new NotSupportedException();

    public static Suggestion Create(
        int id,
        SuggestionType type,
        SuggestionStatus status,
        User user,
        DateTime createdAt,
        DateTime updatedAt,
        SuggestionVotes votes,
        Shop? shop,
        string proposedValue,
        string? additionalDataJson
    ) => type.Id switch
    {
        1 => new NewShopSuggestion(id, type, status, user, createdAt, updatedAt, votes, proposedValue, additionalDataJson!),
        2 => new EditShopNameSuggestion(id, type, status, user, createdAt, updatedAt, votes, shop!, proposedValue),
        3 => new EditShopAddressSuggestion(id, type, status, user, createdAt, updatedAt, votes, shop!, proposedValue),
        4 => new EditShopOpeningHours(id, type, status, user, createdAt, updatedAt, votes, shop!, additionalDataJson!),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}