using System;

namespace SzivarClubManager.Models.Suggestions;

public sealed class SuggestionVotes
{
    public int SuggestionId { get; }
    public int LikeCount { get; }
    public int DislikeCount { get; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }

    public int TotalVotes => LikeCount - DislikeCount;

    public SuggestionVotes(int suggestionId, int likeCount, int dislikeCount, DateTime createdAt, DateTime updatedAt)
    {
        SuggestionId = suggestionId;
        LikeCount = likeCount;
        DislikeCount = dislikeCount;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}