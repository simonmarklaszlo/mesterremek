namespace SzivarClubManager.Datasources.Change;

public interface IDataChanges
{
    public int GetNewCount();
    public int GetEditedCount();
    public int GetDeletedCount();
    public void Apply();
    public void Drop();
}