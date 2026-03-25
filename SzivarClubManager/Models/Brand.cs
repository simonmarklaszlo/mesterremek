using SzivarClubManager.SourceGeneration.Model;

namespace SzivarClubManager.Models;

[Model]
public class Brand : IModel
{
    public int Id { get; }
    public string Name { get; protected set; }

    public Brand(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public string ToCopiableString() => $"{Id} {Name}";
    public override string ToString() => Name;
    public override bool Equals(object? obj) => obj is Brand brand && brand.Id == Id;
    public static bool operator ==(Brand? a, Brand? b) => a is not null && a.Equals(b);
    public static bool operator !=(Brand? a, Brand? b) => !(a == b);
    public override int GetHashCode() => Id.GetHashCode();
    public static Brand CreateNew(string name) => new(-1, name);
}