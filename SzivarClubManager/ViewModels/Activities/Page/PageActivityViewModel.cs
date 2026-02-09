using CommunityToolkit.Mvvm.ComponentModel;

namespace SzivarClubManager.ViewModels.Activities.Page;

public abstract partial class PageActivityViewModel : ActivityViewModel
{
    [ObservableProperty] private object? _viewModel;
}