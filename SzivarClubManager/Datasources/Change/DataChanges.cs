using System.Collections.Generic;
using System.Linq;
using SzivarClubManager.Models;

namespace SzivarClubManager.Datasources.Change;

public class DataChanges<T> : IDataChanges where T : class, IModel
{
    private readonly List<T> _newRecords = [];
    private readonly List<T> _editedRecords = [];
    private readonly List<T> _deletedRecords = [];

    public IReadOnlyList<T> NewRecords => _newRecords.AsReadOnly();
    public IReadOnlyList<T> EditedRecords => _editedRecords.AsReadOnly();
    public IReadOnlyList<T> DeletedRecords => _deletedRecords.AsReadOnly();

    private readonly IModelFactory<T> _factory;

    public DataChanges(IModelFactory<T> factory) => _factory = factory;


    public void Add(T model) => _newRecords.Add(model);
    public void Edit(T model)
    {
        _editedRecords.RemoveAll(x => x.Id == model.Id);
        _editedRecords.Add(model);
    }

    public void Delete(T model) => _deletedRecords.Add(model);
    public void Delete(IEnumerable<T> models) => _deletedRecords.AddRange(models);

    public bool IsEdited(T model) => _editedRecords.Any(x => x.Id == model.Id);
    public bool IsDeleted(T model) => _deletedRecords.Any(x => x.Id == model.Id);

    public int AddCount() => _newRecords.Count;
    public int EditCount() => _editedRecords.Count;
    public int DeleteCount() => _deletedRecords.Count;

    public T? GetEdited(int id) => _editedRecords.LastOrDefault(x => x.Id.Equals(id));
    public T? GetDeleted(int id) => _deletedRecords.LastOrDefault(x => x.Id.Equals(id));


    public void RemoveEdit(T model) => _editedRecords.Remove(model);
    public void RemoveDeleted(T model) => _deletedRecords.Remove(model);
    public void RemoveDeleted(IEnumerable<T> models) => _deletedRecords.RemoveAll(models.Contains);

    public void Apply()
    {
        _factory.DeleteRange(_deletedRecords.DistinctBy(x => x.Id));
        _factory.AddRange(_newRecords.DistinctBy(x => x.Id));
        _factory.EditRange(Enumerable.Reverse(_editedRecords).DistinctBy(x => x.Id));

        ClearLists();
    }

    public void Drop() => ClearLists();

    private void ClearLists()
    {
        _newRecords.Clear();
        _deletedRecords.Clear();
        _editedRecords.Clear();
    }
}