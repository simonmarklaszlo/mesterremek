using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources.Change;

namespace SzivarClubManager.ViewModels.Activities.Change;

public sealed partial class ChangesActivityViewModel : ActivityViewModel
{
    [RelayCommand]
    private void DropAll() => Changes.DropAll();

    [RelayCommand]
    private void ApplyAll() => Changes.ApplyAll();
}