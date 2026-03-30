using SzivarClubManager.Models.Suggestions;

namespace SzivarClubManager.ViewModels.Components.Suggestions.SuggestionContent;

public sealed class NewShopSuggestionViewModel : ViewModelBase
{
    public NewShopSuggestion NewShopSuggestion { get; }
    public ShopOpeningScheduleViewModel ScheduleViewModel { get; }
    public NewShopSuggestionViewModel(NewShopSuggestion newShopSuggestion)
    {
        NewShopSuggestion = newShopSuggestion;
        ScheduleViewModel = new ShopOpeningScheduleViewModel(newShopSuggestion.OpeningSchedule);
    }
}