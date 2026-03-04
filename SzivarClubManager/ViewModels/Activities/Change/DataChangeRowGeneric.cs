using System.Collections.Generic;
using SzivarClubManager.Datasources.Change;
using SzivarClubManager.Models;

namespace SzivarClubManager.ViewModels.Activities.Change;

public class DataChangeRow<T> : DataChangeRow where T : class, IModel
{
    private IReadOnlyList<T> Added { get; set; } = [];
    private IReadOnlyList<T> Edited { get; set; } = [];
    private IReadOnlyList<T> Deleted { get; set; } = [];

    public override string TypeName => typeof(T).Name;

    public override void Refresh()
    {
        Added = Changes.GetAllAdded<T>();
        AddCount = Added.Count;
        Edited = Changes.GetAllEdited<T>();
        EditCount = Edited.Count;
        Deleted = Changes.GetAllDeleted<T>();
        DeleteCount = Deleted.Count;
    }
}