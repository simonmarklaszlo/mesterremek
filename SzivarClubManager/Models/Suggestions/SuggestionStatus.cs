using System;

namespace SzivarClubManager.Models.Suggestions;

public sealed class SuggestionStatus : IModel
{
    public int Id { get; }
    public string Code { get; }
    public string Name { get; }
    public string Description { get; }
    public DateTime CreatedAt { get; }

    public SuggestionStatus(int id, string code, string name, string description, DateTime createdAt)
    {
        Id = id;
        Code = code;
        Name = name;
        Description = description;
        CreatedAt = createdAt;
    }

    public string ToCopiableString() => throw new NotSupportedException();
}