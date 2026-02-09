using System;

namespace SzivarClubManager.Models;

public record User(
    int Id,
    string Name,
    string Email,
    DateTime CreatedAt,
    Role Role
)
{
    public string ToCopiableString() => $"{Id} {Name} {Email} {CreatedAt} {Role}";
    public User Copy() => this with { Role = Role.Copy() };
}