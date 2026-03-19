using CommunityToolkit.Mvvm.ComponentModel;
using SzivarClubManager.Services;

namespace SzivarClubManager.ViewModels.Activities.Page;

public abstract partial class PageActivityViewModel(PopupService popupService) : ActivityViewModel(popupService)
{
    [ObservableProperty] private ViewModelBase? _viewModel;
}