using System.Threading.Tasks;
using SzivarClubManager.Configs;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Models.Suggestions;
using SzivarClubManager.Services;
using SzivarClubManager.SourceGeneration.Activity;

namespace SzivarClubManager.ViewModels.Activities.Suggestions;

[ActivityCollectionItem("Javaslatok", 3)]
public sealed class SuggestionsActivityViewModel(PopupService popupService) : SuggestionActivityBaseViewModel(popupService)
{
    protected override Task<Suggestion[]> GetCurrentPageData() => SuggestionsFactory.GetPage(CurrentPage, GlobalConfig.Instance.UserPreferences.PageSize, SuggestionFilter.Empty);
}