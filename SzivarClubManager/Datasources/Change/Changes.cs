using System;
using System.Collections.Generic;
using SzivarClubManager.Models;
using SzivarClubManager.Models.Editable;

namespace SzivarClubManager.Datasources.Change;

/// <summary>
/// Provides centralized change tracking operations for model instances.
/// </summary>
public static partial class Changes
{
    /// <summary>
    /// Gets the timestamp of the latest change-tracking operation.
    /// </summary>
    public static DateTime LastChange { get; private set; } = DateTime.MinValue;

    /// <summary>
    /// Marks a model instance as newly added.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    /// <param name="item">The added model instance.</param>
    public static void AddNew<T>(T item) where T : class, IModel
    {
        GetDataChange<T>().Add(item);
        LastChange = DateTime.Now;
    }

    /// <summary>
    /// Gets the number of tracked added items for the specified model type.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    public static int AddCount<T>() where T : class, IModel => GetDataChange<T>().AddCount();

    /// <summary>
    /// Gets all tracked added items for the specified model type.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    public static IReadOnlyList<T> GetAllAdded<T>() where T : class, IModel => GetDataChange<T>().NewRecords;


    /// <summary>
    /// Gets the editable representation of a tracked edited model by its identifier.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    /// <param name="id">The identifier of the model.</param>
    /// <returns>The editable model if found; otherwise <see langword="null"/>.</returns>
    public static IEditableModel? GetEdited<T>(int id) where T : class, IModel => GetDataChange<T>().GetEdited(id) as IEditableModel;

    /// <summary>
    /// Removes an item from the tracked edited collection.
    /// </summary>
    /// <see cref=""/>
    /// <typeparam name="T">The model type.</typeparam>
    /// <param name="item">The model instance to remove from edited tracking.</param>
    public static void RemoveEdit<T>(T item) where T : class, IModel
    {
        GetDataChange<T>().RemoveEdit(item);
        LastChange = DateTime.Now;
    }

    /// <summary>
    /// Marks a model instance as edited.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    /// <param name="item">The edited model instance.</param>
    public static void Edit<T>(T item) where T : class, IModel
    {
        GetDataChange<T>().Edit(item);
        LastChange = DateTime.Now;
    }

    /// <summary>
    /// Gets the number of tracked edited items for the specified model type.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    public static int EditCount<T>() where T : class, IModel => GetDataChange<T>().EditCount();

    /// <summary>
    /// Gets all tracked edited items for the specified model type.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    public static IReadOnlyList<T> GetAllEdited<T>() where T : class, IModel => GetDataChange<T>().EditedRecords;


    /// <summary>
    /// Determines whether the specified item is marked as deleted.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    /// <param name="item">The model instance to check.</param>
    public static bool IsDeleted<T>(T item) where T : class, IModel => GetDataChange<T>().IsDeleted(item);

    /// <summary>
    /// Determines whether the specified item is marked as edited.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    /// <param name="item">The model instance to check.</param>
    public static bool IsEdited<T>(T item) where T : class, IModel => GetDataChange<T>().IsEdited(item);

    /// <summary>
    /// Marks a model instance as deleted.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    /// <param name="item">The model instance to mark as deleted.</param>
    public static void Delete<T>(T item) where T : class, IModel
    {
        GetDataChange<T>().Delete(item);
        LastChange = DateTime.Now;
    }

    /// <summary>
    /// Marks multiple model instances as deleted.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    /// <param name="items">The model instances to mark as deleted.</param>
    public static void Delete<T>(IEnumerable<T> items) where T : class, IModel
    {
        GetDataChange<T>().Delete(items);
        LastChange = DateTime.Now;
    }

    /// <summary>
    /// Removes a model instance from the tracked deleted collection.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    /// <param name="item">The model instance to remove from deleted tracking.</param>
    public static void RemoveDeleted<T>(T item) where T : class, IModel
    {
        GetDataChange<T>().RemoveDeleted(item);
        LastChange = DateTime.Now;
    }

    /// <summary>
    /// Removes multiple model instances from the tracked deleted collection.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    /// <param name="items">The model instances to remove from deleted tracking.</param>
    public static void RemoveDeleted<T>(IEnumerable<T> items) where T : class, IModel
    {
        GetDataChange<T>().RemoveDeleted(items);
        LastChange = DateTime.Now;
    }

    /// <summary>
    /// Gets the number of tracked deleted items for the specified model type.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    public static int DeleteCount<T>() where T : class, IModel => GetDataChange<T>().DeleteCount();

    /// <summary>
    /// Gets all tracked deleted items for the specified model type.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    public static IReadOnlyList<T> GetAllDeleted<T>() where T : class, IModel => GetDataChange<T>().DeletedRecords;


    /// <summary>
    /// Clears every tracked change entry for all registered model types.
    /// </summary>
    public static void DropAll()
    {
        foreach (var (_, change) in _dataChanges)
        {
            change.Drop();
        }

        LastChange = DateTime.Now;
    }

    /// <summary>
    /// Applies all tracked changes for every registered model type.
    /// </summary>
    public static void ApplyAll()
    {
        foreach (var (_, change) in _dataChanges)
        {
            change.Apply();
        }

        LastChange = DateTime.Now;
    }
}