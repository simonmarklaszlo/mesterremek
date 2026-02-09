namespace SzivarClubManager.Models;

public record Brand(int Id, string Name)
{
    public override string ToString() => Name;
    public Brand Copy() => new(Id, Name);
}