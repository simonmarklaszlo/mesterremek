using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SzivarClubManager.Datasources.Database.Factories;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models;
using SzivarClubManager.Models.Time;

namespace SzivarClubManager.ViewModels.Components;

public sealed partial class ShopOpeningScheduleViewModel : ViewModelBase
{
    [ObservableProperty] private IReadOnlyList<ShopOpeningHour> _openingHours;
    [ObservableProperty] private bool _showSchedule;
    [ObservableProperty] private bool _showLoading = true;

    private OpeningHourFactory OpeningHourFactory => field ??= (OpeningHourFactory)FactoryProvider.Instance.GetFactory<ShopOpeningHour>();

    public ShopOpeningScheduleViewModel()
    {
        OpeningHours = [];
    }

    public void SetShop(Shop shop)
    {
        if (shop.Schedule is null)
        {
            OpeningHours = [];
            ShowSchedule = false;
            ShowLoading = true;
            _ = LoadSchedule(shop);
        }
        else
        {
            OpeningHours = shop.Schedule.OpeningHours;
            ShowLoading = false;
            ShowSchedule = true;
        }
    }

    private async Task LoadSchedule(Shop shop)
    {
        shop.Schedule = await OpeningHourFactory.GetSchedule(shop);

        OpeningHours = shop.Schedule.OpeningHours;
        ShowLoading = false;
        ShowSchedule = true;
    }
}