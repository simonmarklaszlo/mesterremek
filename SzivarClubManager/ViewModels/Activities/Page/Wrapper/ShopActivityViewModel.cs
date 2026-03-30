using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models;
using SzivarClubManager.Services;
using SzivarClubManager.SourceGeneration.Activity;
using SzivarClubManager.ViewModels.Activities.Page.Data;
using SzivarClubManager.ViewModels.Activities.Page.ItemAdd;
using SzivarClubManager.ViewModels.Activities.Page.ItemEdit;

namespace SzivarClubManager.ViewModels.Activities.Page.Wrapper;

[PageActivityCollectionItem("Boltok", typeof(Shop), 1)]
public sealed class ShopActivityViewModel(
    PopupService popupService,
    IPageFactory<Shop> factory
) : PageActivityViewModel<Shop, ShopFilter, ShopPageDataViewModel, ShopAddViewModel, ShopEditViewModel>(popupService)
{
    protected override ShopPageDataViewModel DataViewModel => field ??= new ShopPageDataViewModel(PopupService, Controller, factory);
    protected override ShopAddViewModel ItemAddViewModel => field ??= new ShopAddViewModel(PopupService);
    protected override ShopEditViewModel ItemEditViewModel => field ??= new ShopEditViewModel(Controller);
    protected override ShopFilter Filter => field ??= new ShopFilter();
}