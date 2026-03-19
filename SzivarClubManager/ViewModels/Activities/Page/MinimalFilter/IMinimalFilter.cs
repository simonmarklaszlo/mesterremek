using CommunityToolkit.Mvvm.Input;

namespace SzivarClubManager.ViewModels.Activities.Page.MinimalFilter;

public interface IMinimalFilterViewModel
{
    public string SearchText { get; set; }
    public IRelayCommand ShowFilterCommand { get; }
    public IRelayCommand TriggerSearchCommand { get; }
    public IRelayCommand ClearFilterCommand { get; }
}