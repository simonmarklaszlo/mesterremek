using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Models;
using SzivarClubManager.ViewModels.Activities.Page.Navigation;

namespace SzivarClubManager.ViewModels.Activities.Page.ItemAdd;

public abstract partial class ItemAddViewModel<T> : ViewModelBase where T : class, IModel
{
    public PageController<T> Controller { get; init; } = null!;
    protected abstract bool CanAdd { get; }

    protected abstract void ResetFields();
    protected abstract void AddNewItemToChanges();

    protected void ReCheckCommandCanExecute() => AddCommand.NotifyCanExecuteChanged();

    [RelayCommand]
    private void Cancel()
    {
        ResetFields();
        Controller.NavigateBack();
    }

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private void Add()
    {
        AddNewItemToChanges();
        Controller.NavigateBack();
    }
}