using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Configs;
using SzivarClubManager.Datasources.Pagination;

namespace SzivarClubManager.ViewModels.Activities;

public partial class PageActivityViewModel<T> : PageActivityBaseViewModel
{
    private readonly IPageFactory<T> _pageFactory;

    public override IRelayCommand LoadFirstPageCmd { get; }
    public override IRelayCommand LoadPreviousPageCmd { get; }
    public override IRelayCommand LoadNextPageCmd { get; }
    public override IRelayCommand LoadLastPageCmd { get; }

    protected PageActivityViewModel(IPageFactory<T> pageFactory)
    {
        _pageFactory = pageFactory;

        LoadFirstPageCmd = LoadFirstPageCommand;
        LoadPreviousPageCmd = LoadPreviousPageCommand;
        LoadNextPageCmd = LoadNextPageCommand;
        LoadLastPageCmd = LoadLastPageCommand;
    }

    public override async Task InitializeAsync()
    {
        if (!IsInitialized) await LoadCurrentPage();
        await base.InitializeAsync();
    }

    private async Task LoadCurrentPage()
    {
        var data = await _pageFactory.GetPage(CurrentPage, GlobalConfig.Instance.UserPreferences.PageSize);
        Data = new ObservableCollection<T>(data);

        IsFirstPageButtonEnabled = CurrentPage > 1;
        IsPreviousPageButtonEnabled = CurrentPage > 1;
        int lastPage = await _pageFactory.GetLastPage(GlobalConfig.Instance.UserPreferences.PageSize);
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
        if (await _pageFactory.PageExists(CurrentPage + 1, GlobalConfig.Instance.UserPreferences.PageSize))
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
        var page = await _pageFactory.GetLastPage(GlobalConfig.Instance.UserPreferences.PageSize);
        CurrentPage = page;
        await LoadCurrentPage();
    }
}