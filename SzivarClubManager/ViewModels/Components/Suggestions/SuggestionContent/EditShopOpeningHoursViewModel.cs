using SzivarClubManager.Models.Suggestions;

namespace SzivarClubManager.ViewModels.Components.Suggestions.SuggestionContent;

public sealed class EditShopOpeningHoursViewModel : ViewModelBase
{
    public EditShopOpeningHours EditShopOpeningHours { get; }
    public ShopOpeningScheduleViewModel NewScheduleViewModel { get; }
    public ShopOpeningScheduleViewModel OldScheduleViewModel { get; }

    public EditShopOpeningHoursViewModel(EditShopOpeningHours editShopOpeningHours)
    {
        EditShopOpeningHours = editShopOpeningHours;
        NewScheduleViewModel = new ShopOpeningScheduleViewModel(editShopOpeningHours.NewOpeningHours);
        OldScheduleViewModel = new ShopOpeningScheduleViewModel();
        OldScheduleViewModel.SetShop(editShopOpeningHours.Shop);
    }
}