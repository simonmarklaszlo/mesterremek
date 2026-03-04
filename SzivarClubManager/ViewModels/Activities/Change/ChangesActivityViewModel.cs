using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources.Change;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;

namespace SzivarClubManager.ViewModels.Activities.Change;

[ActivityCollectionItem]
public sealed partial class ChangesActivityViewModel : ActivityViewModel
{
    public ObservableCollection<DataChangeRow> DataChangeRows { get; } =
    [
        new DataChangeRow<Cigar>(),
        new DataChangeRow<Shop>(),
        new DataChangeRow<User>()
    ];

    private DateTime _lastCheck = DateTime.MinValue;

    public override void OnOpening()
    {
        if (Changes.LastChange > _lastCheck)
        {
            _lastCheck = Changes.LastChange;
            RefreshData();
        }
    }

    private void RefreshData()
    {
        foreach (var row in DataChangeRows)
        {
            row.Refresh();
        }
    }


    [RelayCommand]
    private void DropAll()
    {
        Changes.DropAll();
        RefreshData();
    }

    [RelayCommand]
    private void ApplyAll()
    {
        Changes.ApplyAll();
        RefreshData();
    }
}