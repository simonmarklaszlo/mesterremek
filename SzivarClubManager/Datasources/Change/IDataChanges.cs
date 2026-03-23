namespace SzivarClubManager.Datasources.Change;

public interface IDataChanges
{
    /// <summary>
    /// Write all tracked changes to the underlying data source and clears the pending change lists.
    /// </summary>
    /// <remarks>
    /// Changes are applied in the following order: delete, add, then update.
    /// Model id resolves duplicates.
    /// </remarks>
    void Apply();

    /// <summary>
    /// Discards all tracked changes.
    /// </summary>
    void Drop();
}