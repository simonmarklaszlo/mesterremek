using SzivarClubManager.Models.Suggestions;

namespace SzivarClubManager.ViewModels.Components.Suggestions.SuggestionContent;

public sealed class EditShopNameSuggestionViewModel : ViewModelBase
{
    public EditShopNameSuggestion EditShopNameSuggestion { get; }

    public EditShopNameSuggestionViewModel(EditShopNameSuggestion editShopNameSuggestion)
    {
        EditShopNameSuggestion = editShopNameSuggestion;
    }
}