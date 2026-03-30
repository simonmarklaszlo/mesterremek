using SzivarClubManager.Models.Suggestions;

namespace SzivarClubManager.ViewModels.Components.Suggestions.SuggestionContent;

public sealed class EditShopAddressSuggestionViewModel : ViewModelBase
{
    public EditShopAddressSuggestion EditShopAddressSuggestion { get; }

    public EditShopAddressSuggestionViewModel(EditShopAddressSuggestion editShopAddressSuggestion)
    {
        EditShopAddressSuggestion = editShopAddressSuggestion;
    }
}