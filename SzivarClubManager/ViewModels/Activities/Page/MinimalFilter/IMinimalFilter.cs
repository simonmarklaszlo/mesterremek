using CommunityToolkit.Mvvm.Input;

namespace SzivarClubManager.ViewModels.Activities.Page.MinimalFilter;

public interface IMinimalFilterViewModel
{
    string SearchText { get; set; }
    IRelayCommand ShowFilterCommand { get; }
    IRelayCommand TriggerSearchCommand { get; }
    IRelayCommand ClearFilterCommand { get; }
}