namespace SzivarClubManager.Models;

public interface IModel
{
    int Id { get; }

    string ToCopiableString();
}