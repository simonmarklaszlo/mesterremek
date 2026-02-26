using System.Collections.Generic;
using SzivarClubManager.Models;
using SzivarClubManager.Models.Editable;

namespace SzivarClubManager.Datasources.Change;

public static partial class Changes
{
    public static bool IsDeleted<T>(T item) where T : class, IModel => GetDataChange<T>().IsDeleted(item);
    public static bool IsEdited<T>(T item) where T : class, IModel => GetDataChange<T>().IsEdited(item);


    public static IEditableModel? GetEdited<T>(int id) where T : class, IModel => GetDataChange<T>().GetEdited(id) as IEditableModel;


    public static void RemoveEdit<T>(T item) where T : class, IModel => GetDataChange<T>().RemoveEdit(item);
    public static void WriteEdit<T>(T item) where T : class, IModel => GetDataChange<T>().WriteEdit(item);
    public static void Delete<T>(T item) where T : class, IModel => GetDataChange<T>().Delete(item);
    public static void Delete<T>(IEnumerable<T> items) where T : class, IModel => GetDataChange<T>().Delete(items);
    public static void RemoveDeleted<T>(T item) where T : class, IModel => GetDataChange<T>().RemoveDeleted(item);
    public static void RemoveDeleted<T>(IEnumerable<T> items) where T : class, IModel => GetDataChange<T>().RemoveDeleted(items);


    public static void DropAll()
    {
        foreach (var (_, change)in _dataChanges)
        {
            change.Drop();
        }
    }

    public static void ApplyAll()
    {
        foreach (var (_, change) in _dataChanges)
        {
            change.Apply();
        }
    }
}