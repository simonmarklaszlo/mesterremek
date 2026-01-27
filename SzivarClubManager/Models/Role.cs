namespace SzivarClubManager.Models;

public record Role(int Id, string Name)
{
    public override string ToString()
    {
        return Name;
    }
}