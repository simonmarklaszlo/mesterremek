using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Configs;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models.Suggestions;
using SzivarClubManager.Services;
using SzivarClubManager.SourceGeneration.Activity;

namespace SzivarClubManager.ViewModels.Activities.Suggestions;

[ActivityCollectionItem("Suggestions", 3)]
public sealed partial class SuggestionsActivityViewModel(PopupService popupService) : ActivityViewModel(popupService)
{
    public ObservableCollection<Suggestion> Suggestions { get; } = [];
    private readonly IPageFactory<Suggestion> _suggestionsFactory = FactoryProvider.Instance.GetPageFactory<Suggestion>();


    private int _currentPage = 1;
    private bool _isLoading = false;
    private bool _hasMore = true;

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
            foreach (var item in items)
                Suggestions.Add(item);

            _currentPage++;
        }

        _isLoading = false;
    }
}