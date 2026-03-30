using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Services;

namespace SzivarClubManager.ViewModels.Activities.Page.ItemAdd;

public abstract partial class ItemAddViewModel : ViewModelBase
{
    protected PopupService PopupService { get; }
    protected abstract bool CanAdd { get; }

    protected ItemAddViewModel(PopupService popupService)
    {
        PopupService = popupService;
    }

    protected abstract void ResetFields();
    protected abstract void AddNewItemToChanges();

    protected void ReCheckCommandCanExecute() => AddCommand.NotifyCanExecuteChanged();

    [RelayCommand]
    private void Cancel()
    {
        ResetFields();
        PopupService.ClosePopup();
    }

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private void Add()
    {
        AddNewItemToChanges();
        PopupService.ClosePopup();
    }
}