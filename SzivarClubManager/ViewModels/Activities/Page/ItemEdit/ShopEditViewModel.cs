using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Models;
using SzivarClubManager.ViewModels.Activities.Page.Navigation;

namespace SzivarClubManager.ViewModels.Activities.Page.ItemEdit;

public sealed partial class ShopEditViewModel : ViewModelBase
{
    [ObservableProperty] private Shop _selectedItem;
    [ObservableProperty] private Shop _copy;
    [ObservableProperty] private bool _isEditMode;

    private readonly PageController<Shop> _controller;

    public ShopEditViewModel(Shop item, PageController<Shop> controller)
    {
        SelectedItem = item;
        Copy = item.Copy();
        _controller = controller;
    }

    partial void OnSelectedItemChanged(Shop value) => Copy = value.Copy();

    public void SetIntent(NavigationIntent intent)
    {
        switch (intent)
        {
            case NavigationIntent.View:
                IsEditMode = false;
                break;
            case NavigationIntent.Edit:
                IsEditMode = true;
                break;
            case NavigationIntent.Delete:
                IsEditMode = false;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(intent), intent, null);
        }
    }

    [RelayCommand]
    private void NavigateBack()
    {
        Copy = SelectedItem.Copy();
        _controller.NavigateBack();
    }

    [RelayCommand]
    private void SaveChanges() { }
}