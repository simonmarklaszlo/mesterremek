using System.Threading.Tasks;
using SzivarClubManager.Configs;
using SzivarClubManager.Datasources.Database.Factories;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Models.Suggestions;
using SzivarClubManager.Services;
using SzivarClubManager.SourceGeneration.Activity;

namespace SzivarClubManager.ViewModels.Activities.Suggestions;

[ActivityCollectionItem("Problémás javaslatok", 3)]
public sealed class FlaggedSuggestionsActivityViewModel(PopupService popupService) : SuggestionActivityBaseViewModel(popupService)
{
    private SuggestionFactory SuggestionFactory => field ??= (SuggestionFactory)SuggestionsFactory;
    protected override Task<Suggestion[]> GetCurrentPageData()
    {
        return SuggestionFactory.GetPageOnFlagged(CurrentPage, GlobalConfig.Instance.UserPreferences.PageSize, SuggestionFilter.Empty);
    }
}