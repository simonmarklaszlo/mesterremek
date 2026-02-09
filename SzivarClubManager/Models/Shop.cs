using System;

namespace SzivarClubManager.Models;

public record Shop(
    int Id,
    string Name,
    string Address,
    string City,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    CustomPgPoint Location
)
{
    public string ToCopiableString() => $"{Id} {Name} {Address} {City} {CreatedAt} {UpdatedAt} {Location}";
    public Shop Copy() => this with { Location = Location.Copy() };
}