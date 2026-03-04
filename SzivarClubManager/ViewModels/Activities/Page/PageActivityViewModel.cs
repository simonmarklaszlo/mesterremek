using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SzivarClubManager.ViewModels.Activities.Page;

public abstract partial class PageActivityViewModel : ActivityViewModel
{
    [ObservableProperty] private ViewModelBase? _viewModel;
}