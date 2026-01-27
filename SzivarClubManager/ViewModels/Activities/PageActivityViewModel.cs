using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Configs;
using SzivarClubManager.Datasources;

namespace SzivarClubManager.ViewModels.Activities;

public partial class PageActivityViewModel<T> : PageActivityBaseViewModel
{
    [ObservableProperty] private ObservableCollection<T> _dataObservable;
    [ObservableProperty] private int _currentPageObservable;
    [ObservableProperty] private bool _isFirstPageButtonEnabledObservable;
    [ObservableProperty] private bool _isPreviousPageButtonEnabledObservable;
    [ObservableProperty] private bool _isNextPageButtonEnabledObservable;
    [ObservableProperty] private bool _isLastPageButtonEnabledObservable;

    private readonly Func<int, int, Task<bool>> _pageExistsFunc;
    private readonly Func<int, Task<int>> _lastPageFunc;
    private readonly Func<int, int, Task<T[]>> _loadPageFunc;

    protected PageActivityViewModel(IDataSource dataSource, Func<int, int, Task<bool>> pageExistsFunc, Func<int, Task<int>> lastPageFunc, Func<int, int, Task<T[]>> loadPageFunc) : base(dataSource)
    {
        _pageExistsFunc = pageExistsFunc;
        _lastPageFunc = lastPageFunc;
        _loadPageFunc = loadPageFunc;

        _dataObservable = [];
        _ = InitAsync();
    }

    private async Task InitAsync() => await LoadCurrentPage();

    private async Task LoadCurrentPage()
    {
        var data = await _loadPageFunc(CurrentPageObservable, GlobalConfig.Instance.UserPreferences.PageSize);
        DataObservable = new ObservableCollection<T>(data);

        IsFirstPageButtonEnabledObservable = CurrentPageObservable > 1;
        IsPreviousPageButtonEnabledObservable = CurrentPageObservable > 1;
        int lastPage = await DataSource.GetLastUserPage(GlobalConfig.Instance.UserPreferences.PageSize);
        IsNextPageButtonEnabledObservable = lastPage > CurrentPageObservable;
        IsLastPageButtonEnabledObservable = lastPage > CurrentPageObservable;
    }

    [RelayCommand]
    private async Task LoadPreviousPage()
    {
        if (CurrentPageObservable > 1)
        {
            CurrentPageObservable--;
            await LoadCurrentPage();
        }
    }

    [RelayCommand]
    private async Task LoadNextPage()
    {
        if (await _pageExistsFunc(CurrentPageObservable + 1, GlobalConfig.Instance.UserPreferences.PageSize))
        {
            CurrentPageObservable++;
            await LoadCurrentPage();
        }
    }

    [RelayCommand]
    private async Task LoadFirstPage()
    {
        CurrentPageObservable = 1;
        await LoadCurrentPage();
    }

    [RelayCommand]
    private async Task LoadLastPage()
    {
        var page = await _lastPageFunc(GlobalConfig.Instance.UserPreferences.PageSize);
        CurrentPageObservable = page;
        await LoadCurrentPage();
    }

    public override IEnumerable Data => DataObservable;
    public override int CurrentPage => CurrentPageObservable;
    public override bool IsFirstPageButtonEnabled => IsFirstPageButtonEnabledObservable;
    public override bool IsPreviousPageButtonEnabled => IsPreviousPageButtonEnabledObservable;
    public override bool IsNextPageButtonEnabled => IsNextPageButtonEnabledObservable;
    public override bool IsLastPageButtonEnabled => IsLastPageButtonEnabledObservable;
    public override IRelayCommand LoadFirstPageCmd => LoadFirstPageCommand;
    public override IRelayCommand LoadPreviousPageCmd => LoadPreviousPageCommand;
    public override IRelayCommand LoadNextPageCmd => LoadNextPageCommand;
    public override IRelayCommand LoadLastPageCmd => LoadLastPageCommand;
}