using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources.Change;
using SzivarClubManager.Services;
using SzivarClubManager.SourceGeneration.Activity;

namespace SzivarClubManager.ViewModels.Activities.Change;

[ActivityCollectionItem("Módosítások", 2)]
public sealed partial class ChangesActivityViewModel(PopupService popupService) : ActivityViewModel(popupService)
{
    public ObservableCollection<DataChangeRow> DataChangeRows { get; } = GetDataChangeRows();

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