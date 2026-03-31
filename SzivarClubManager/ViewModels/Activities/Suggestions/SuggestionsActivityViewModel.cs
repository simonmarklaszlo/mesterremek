using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Configs;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models.Suggestions;
using SzivarClubManager.Services;
using SzivarClubManager.SourceGeneration.Activity;
using SuggestionWrapperViewModel = SzivarClubManager.ViewModels.Components.Suggestions.SuggestionWrapperViewModel;

namespace SzivarClubManager.ViewModels.Activities.Suggestions;

[ActivityCollectionItem("Javaslatok", 3)]
public sealed partial class SuggestionsActivityViewModel(PopupService popupService) : ActivityViewModel(popupService)
{
    public ObservableCollection<SuggestionWrapperViewModel> SuggestionsViewModels { get; } = [];
    private readonly IPageFactory<Suggestion> _suggestionsFactory = FactoryProvider.Instance.GetPageFactory<Suggestion>();

    [ObservableProperty] private bool _showData = true;
    [ObservableProperty] private bool _showPlaceholder;


    private int _currentPage = 1;
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

        _isLoading = true;

        var items = await _suggestionsFactory.GetPage(_currentPage, GlobalConfig.Instance.UserPreferences.PageSize, SuggestionFilter.Empty);

        if (items.Length == 0)
        {
            _hasMore = false;
        }
        else
        {
            foreach (var suggestion in items) SuggestionsViewModels.Add(new SuggestionWrapperViewModel(suggestion, OnSuggestionApproved));

            _currentPage++;
        }

        if (_currentPage == 1 && items.Length == 0)
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

    private async Task OnSuggestionApproved()
    {
        SuggestionsViewModels.Clear();
        _currentPage = 1;
        await LoadNextPage();
    }
}