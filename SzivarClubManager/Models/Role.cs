using SzivarClubManager.SourceGeneration.Model;

namespace SzivarClubManager.Models;

[Model(ModelOptions.Unchangeable)]
public class Role : IModel
{
    public int Id { get; }
    public string Name { get; }

    public Role(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public Role Copy() => new(Id, Name);
    public string ToCopiableString() => $"{Id} {Name}";
    public override string ToString() => Name;
    public override bool Equals(object? obj) => obj is Role role && role.Id == Id;
    public static bool operator ==(Role? a, Role? b) => a is not null && a.Equals(b);
    public static bool operator !=(Role? a, Role? b) => !(a == b);
    public override int GetHashCode() => Id.GetHashCode();
}