using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models.Suggestions;
using SzivarClubManager.Services;
using SzivarClubManager.ViewModels.Components.Suggestions;

namespace SzivarClubManager.ViewModels.Activities.Suggestions;

public abstract partial class SuggestionActivityBaseViewModel(PopupService popupService) : ActivityViewModel(popupService)
{
    public ObservableCollection<SuggestionWrapperViewModel> SuggestionsViewModels { get; } = [];
    protected IPageFactory<Suggestion> SuggestionsFactory { get; } = FactoryProvider.Instance.GetPageFactory<Suggestion>();

    [ObservableProperty] private bool _showData = true;
    [ObservableProperty] private bool _showPlaceholder;


    protected int CurrentPage { get; private set; } = 1;
    private bool _isLoading = false;
    private bool _hasMore = true;

    public override void OnOpening()
    {
        if (ShowPlaceholder) _ = LoadNextPage();
    }

    [RelayCommand]
    private async Task LoadNextPage()
    {
        if (_isLoading || !_hasMore) return;

        await Task.Delay(50);

        _isLoading = true;

        var items = await GetCurrentPageData();

        if (items.Length == 0)
        {
            _hasMore = false;
        }
        else
        {
            foreach (var suggestion in items) SuggestionsViewModels.Add(new SuggestionWrapperViewModel(suggestion, OnSuggestionApproved));

            CurrentPage++;
        }

        if (CurrentPage == 1 && items.Length == 0)
        {
            ShowData = false;
            ShowPlaceholder = true;
        }
        else if (ShowPlaceholder)
        {
            ShowData = true;
            ShowPlaceholder = false;
        }

        _isLoading = false;
    }

    protected abstract Task<Suggestion[]> GetCurrentPageData();

    private async Task OnSuggestionApproved()
    {
        SuggestionsViewModels.Clear();
        CurrentPage = 1;
        _hasMore = true;
        await LoadNextPage();
    }
}