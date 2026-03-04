namespace SzivarClubManager.Datasources.Change;

public interface IDataChanges
{
    public int AddCount();
    public int EditCount();
    public int DeleteCount();
    public void Apply();
    public void Drop();
}