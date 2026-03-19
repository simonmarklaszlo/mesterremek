using System;
using SzivarClubManager.Models.Time;

namespace SzivarClubManager.Models;

public class Shop : IModel
{
    public int Id { get; }
    public string Name { get; protected set; }
    public string Address { get; protected set; }
    public string City { get; protected set; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }
    public CustomPgPoint Location { get; protected set; }
    public ShopOpeningSchedule? Schedule { get; set; } = null;

    public Shop(int id, string name, string address, string city, DateTime createdAt, DateTime updatedAt, CustomPgPoint location)
    {
        Id = id;
        Name = name;
        Address = address;
        City = city;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        Location = location;
    }

    public string ToCopiableString() => $"{Id} {Name} {Address} {City} {CreatedAt} {UpdatedAt} {Location}";
    public override string ToString() => Name;
    public override bool Equals(object? obj) => obj is Shop shop && shop.Id == Id;
    public override int GetHashCode() => Id.GetHashCode();
    public static Shop CreateNew(string name, string address, string city, CustomPgPoint location) => new(-1, name, address, city, DateTime.Now, DateTime.Now, location);
}