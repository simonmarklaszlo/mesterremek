using System;

namespace SzivarClubManager.Models.Suggestions;

public sealed class SuggestionVotes : IModel
{
    /// <summary>
    /// Suggestion's id
    /// </summary>
    public int Id { get; }

    public int LikeCount { get; }
    public int DislikeCount { get; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }

    public SuggestionVotes(int id, int likeCount, int dislikeCount, DateTime createdAt, DateTime updatedAt)
    {
        Id = id;
        LikeCount = likeCount;
        DislikeCount = dislikeCount;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public string ToCopiableString() => throw new NotSupportedException();
}