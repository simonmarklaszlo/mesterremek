using System;
using System.Collections.Generic;
using SzivarClubManager.Models;
using SzivarClubManager.Models.Editable;

namespace SzivarClubManager.Datasources.Change;

public static partial class Changes
{
    public static DateTime LastChange { get; private set; } = DateTime.MinValue;
    public static void AddNew<T>(T item) where T : class, IModel
    {
        GetDataChange<T>().Add(item);
        LastChange = DateTime.Now;
    }

    public static int AddCount<T>() where T : class, IModel => GetDataChange<T>().AddCount();
    public static IReadOnlyList<T> GetAllAdded<T>() where T : class, IModel => GetDataChange<T>().NewRecords;


    public static IEditableModel? GetEdited<T>(int id) where T : class, IModel => GetDataChange<T>().GetEdited(id) as IEditableModel;
    public static void RemoveEdit<T>(T item) where T : class, IModel
    {
        GetDataChange<T>().RemoveEdit(item);
        LastChange = DateTime.Now;
    }

    public static void Edit<T>(T item) where T : class, IModel
    {
        GetDataChange<T>().Edit(item);
        LastChange = DateTime.Now;
    }
    public static int EditCount<T>() where T : class, IModel => GetDataChange<T>().EditCount();
    public static IReadOnlyList<T> GetAllEdited<T>() where T : class, IModel => GetDataChange<T>().EditedRecords;


    public static bool IsDeleted<T>(T item) where T : class, IModel => GetDataChange<T>().IsDeleted(item);
    public static bool IsEdited<T>(T item) where T : class, IModel => GetDataChange<T>().IsEdited(item);
    public static void Delete<T>(T item) where T : class, IModel
    {
        GetDataChange<T>().Delete(item);
        LastChange = DateTime.Now;
    }

    public static void Delete<T>(IEnumerable<T> items) where T : class, IModel
    {
        GetDataChange<T>().Delete(items);
        LastChange = DateTime.Now;
    }

    public static void RemoveDeleted<T>(T item) where T : class, IModel
    {
        GetDataChange<T>().RemoveDeleted(item);
        LastChange = DateTime.Now;
    }

    public static void RemoveDeleted<T>(IEnumerable<T> items) where T : class, IModel
    {
        GetDataChange<T>().RemoveDeleted(items);
        LastChange = DateTime.Now;
    }

    public static int DeleteCount<T>() where T : class, IModel => GetDataChange<T>().DeleteCount();
    public static IReadOnlyList<T> GetAllDeleted<T>() where T : class, IModel => GetDataChange<T>().DeletedRecords;


    public static void DropAll()
    {
        foreach (var (_, change)in _dataChanges)
        {
            change.Drop();
        }

        LastChange = DateTime.Now;
    }

    public static void ApplyAll()
    {
        foreach (var (_, change) in _dataChanges)
        {
            change.Apply();
        }

        LastChange = DateTime.Now;
    }
}