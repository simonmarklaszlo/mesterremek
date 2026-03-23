using System.Collections.Generic;
using System.Linq;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models;

namespace SzivarClubManager.Datasources.Change;

/// <summary>
/// Tracks pending create, update, and delete operations for models before writing them to a data source.
/// </summary>
/// <typeparam name="T">The model type.</typeparam>
public class DataChanges<T> : IDataChanges where T : class, IModel
{
    private readonly List<T> _newRecords = [];
    private readonly List<T> _editedRecords = [];
    private readonly List<T> _deletedRecords = [];

    /// <summary>
    /// Gets the models marked for insertion.
    /// </summary>
    public IReadOnlyList<T> NewRecords => _newRecords.AsReadOnly();

    /// <summary>
    /// Gets the models marked for update.
    /// </summary>
    public IReadOnlyList<T> EditedRecords => _editedRecords.AsReadOnly();

    /// <summary>
    /// Gets the models marked for deletion.
    /// </summary>
    public IReadOnlyList<T> DeletedRecords => _deletedRecords.AsReadOnly();

    private readonly IModelFactory<T> _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataChanges{T}"/> class.
    /// </summary>
    /// <param name="factory">The factory used to apply pending changes.</param>
    public DataChanges(IModelFactory<T> factory) => _factory = factory;


    /// <summary>
    /// Marks a model for insertion.
    /// </summary>
    /// <param name="model">The model to add.</param>
    public void Add(T model) => _newRecords.Add(model);

    /// <summary>
    /// Marks a model for update and replaces any previously tracked update with the same identifier.
    /// </summary>
    /// <param name="model">The model to update.</param>
    public void Edit(T model)
    {
        _editedRecords.RemoveAll(x => x.Id == model.Id);
        _editedRecords.Add(model);
    }

    /// <summary>
    /// Marks a model for deletion.
    /// </summary>
    /// <param name="model">The model to delete.</param>
    public void Delete(T model) => _deletedRecords.Add(model);

    /// <summary>
    /// Marks multiple models for deletion.
    /// </summary>
    /// <param name="models">The models to delete.</param>
    public void Delete(IEnumerable<T> models) => _deletedRecords.AddRange(models);

    /// <summary>
    /// Determines whether a model is currently marked for update.
    /// </summary>
    /// <param name="model">The model to check.</param>
    /// <returns><see langword="true"/> if the model is marked for update; otherwise, <see langword="false"/>.</returns>
    public bool IsEdited(T model) => _editedRecords.Any(x => x.Id == model.Id);

    /// <summary>
    /// Determines whether a model is currently marked for deletion.
    /// </summary>
    /// <param name="model">The model to check.</param>
    /// <returns><see langword="true"/> if the model is marked for deletion; otherwise, <see langword="false"/>.</returns>
    public bool IsDeleted(T model) => _deletedRecords.Any(x => x.Id == model.Id);

    /// <summary>
    /// Gets the number of models marked for insertion.
    /// </summary>
    public int AddCount() => _newRecords.Count;

    /// <summary>
    /// Gets the number of models marked for update.
    /// </summary>
    public int EditCount() => _editedRecords.Count;

    /// <summary>
    /// Gets the number of models marked for deletion.
    /// </summary>
    public int DeleteCount() => _deletedRecords.Count;

    /// <summary>
    /// Gets the latest tracked update for the specified identifier.
    /// </summary>
    /// <param name="id">The model id.</param>
    /// <returns>The tracked updated model, or <see langword="null"/> if not found.</returns>
    public T? GetEdited(int id) => _editedRecords.LastOrDefault(x => x.Id.Equals(id));

    /// <summary>
    /// Gets the latest tracked deletion for the specified identifier.
    /// </summary>
    /// <param name="id">The model id.</param>
    /// <returns>The tracked deleted model, or <see langword="null"/> if not found.</returns>
    public T? GetDeleted(int id) => _deletedRecords.LastOrDefault(x => x.Id.Equals(id));


    /// <summary>
    /// Removes a tracked update.
    /// </summary>
    /// <param name="model">The model update to remove.</param>
    public void RemoveEdit(T model) => _editedRecords.Remove(model);

    /// <summary>
    /// Removes a tracked deletion.
    /// </summary>
    /// <param name="model">The model deletion to remove.</param>
    public void RemoveDeleted(T model) => _deletedRecords.Remove(model);

    /// <summary>
    /// Removes multiple tracked deletions.
    /// </summary>
    /// <param name="models">The model deletions to remove.</param>
    public void RemoveDeleted(IEnumerable<T> models) => _deletedRecords.RemoveAll(models.Contains);

    /// <inheritdoc/>
    public void Apply()
    {
        _factory.DeleteRange(_deletedRecords.DistinctBy(x => x.Id));
        _factory.AddRange(_newRecords.DistinctBy(x => x.Id));
        _factory.EditRange(Enumerable.Reverse(_editedRecords).DistinctBy(x => x.Id));

        ClearLists();
    }

    /// <inheritdoc/>
    public void Drop() => ClearLists();

    private void ClearLists()
    {
        _newRecords.Clear();
        _deletedRecords.Clear();
        _editedRecords.Clear();
    }
}