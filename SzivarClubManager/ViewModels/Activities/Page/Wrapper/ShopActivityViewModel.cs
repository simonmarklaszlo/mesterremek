using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models;
using SzivarClubManager.Services;
using SzivarClubManager.SourceGeneration;
using SzivarClubManager.ViewModels.Activities.Page.Data;
using SzivarClubManager.ViewModels.Activities.Page.ItemAdd;
using SzivarClubManager.ViewModels.Activities.Page.ItemEdit;

namespace SzivarClubManager.ViewModels.Activities.Page.Wrapper;

[PageActivityCollectionItem(typeof(Shop), "Shops", 1)]
public sealed class ShopActivityViewModel(
    PopupService popupService,
    IPageFactory<Shop> factory
) : PageActivityViewModel<Shop, ShopFilter, ShopPageDataViewModel, ShopAddViewModel, ShopEditViewModel>(popupService, factory);