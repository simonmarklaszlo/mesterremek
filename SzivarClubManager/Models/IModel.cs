namespace SzivarClubManager.Models;

public interface IModel
{
    /// <summary>
    /// Main identifier of the model.
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Returns a string representation of the model that can be copied to clipboard.
    /// </summary>
    /// <returns> A string representation of the model. </returns>
    string ToCopiableString();
}