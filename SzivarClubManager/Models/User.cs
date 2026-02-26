using System;

namespace SzivarClubManager.Models;

public class User : IModel
{
    public int Id { get; }
    public string Name { get; protected set; }
    public string Email { get; protected set; }
    public DateTime CreatedAt { get; }
    public Role Role { get; protected set; }

    public User(int id, string name, string email, DateTime createdAt, Role role)
    {
        Id = id;
        Name = name;
        Email = email;
        CreatedAt = createdAt;
        Role = role;
    }

    public string ToCopiableString() => $"{Id} {Name} {Email} {CreatedAt} {Role}";
    public override string ToString() => Name;
    public override bool Equals(object? obj) => obj is User user && user.Id == Id;
    public override int GetHashCode() => Id.GetHashCode();
}