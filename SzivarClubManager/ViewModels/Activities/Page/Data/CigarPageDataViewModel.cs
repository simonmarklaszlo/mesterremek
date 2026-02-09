using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Configs;
using SzivarClubManager.Datasources;
using SzivarClubManager.Helper;
using SzivarClubManager.Models;
using SzivarClubManager.ViewModels.Activities.Page.Navigation;

namespace SzivarClubManager.ViewModels.Activities.Page.Data;

public sealed partial class CigarPageDataViewModel : ViewModelBase
{
    [ObservableProperty] private Cigar[] _currentPageData;
    [ObservableProperty] private int _currentPage = 1;

    [ObservableProperty] private bool _isFirstPageButtonEnabled;
    [ObservableProperty] private bool _isPreviousPageButtonEnabled;
    [ObservableProperty] private bool _isNextPageButtonEnabled;
    [ObservableProperty] private bool _isLastPageButtonEnabled;

    [ObservableProperty] private Cigar? _selectedItem;

    private readonly IPageFactory<Cigar> _factory;
    private readonly PageController<Cigar> _controller;

    public CigarPageDataViewModel(IPageFactory<Cigar> factory, PageController<Cigar> controller)
    {
        _factory = factory;
        _controller = controller;
        _currentPageData = [];
    }

    public Task InitializeAsync() => LoadCurrentPage();

    private async Task LoadCurrentPage()
    {
        CurrentPageData = await _factory.GetPage(CurrentPage, GlobalConfig.Instance.UserPreferences.PageSize);

        IsFirstPageButtonEnabled = CurrentPage > 1;
        IsPreviousPageButtonEnabled = CurrentPage > 1;
        int lastPage = await _factory.GetLastPage(GlobalConfig.Instance.UserPreferences.PageSize);
        IsNextPageButtonEnabled = lastPage > CurrentPage;
        IsLastPageButtonEnabled = lastPage > CurrentPage;
    }

    [RelayCommand]
    private async Task LoadPreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadCurrentPage();
        }
    }

    [RelayCommand]
    private async Task LoadNextPage()
    {
        if (await _factory.PageExists(CurrentPage + 1, GlobalConfig.Instance.UserPreferences.PageSize))
        {
            CurrentPage++;
            await LoadCurrentPage();
        }
    }

    [RelayCommand]
    private async Task LoadFirstPage()
    {
        CurrentPage = 1;
        await LoadCurrentPage();
    }

    [RelayCommand]
    private async Task LoadLastPage()
    {
        CurrentPage = await _factory.GetLastPage(GlobalConfig.Instance.UserPreferences.PageSize);
        await LoadCurrentPage();
    }


    [RelayCommand]
    private void ViewItem()
    {
        if (SelectedItem is not null) _controller.SelectItem(SelectedItem, NavigationIntent.View);
    }


    [RelayCommand]
    private void EditItem()
    {
        if (SelectedItem is not null) _controller.SelectItem(SelectedItem, NavigationIntent.Edit);
    }

    [RelayCommand]
    private void DeleteItem()
    {
        if (SelectedItem is not null) _controller.SelectItem(SelectedItem, NavigationIntent.Delete);
    }


    [RelayCommand]
    private Task CopyValue(object? value) => Clipboard.CopyText(value?.ToString());

    [RelayCommand]
    private Task CopyRowValue(Cigar? item) => Clipboard.CopyText(item?.ToCopiableString());
}