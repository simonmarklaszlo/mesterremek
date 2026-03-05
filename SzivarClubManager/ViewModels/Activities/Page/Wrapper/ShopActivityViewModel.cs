using SzivarClubManager.Datasources;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;
using SzivarClubManager.ViewModels.Activities.Page.Data;
using SzivarClubManager.ViewModels.Activities.Page.ItemAdd;
using SzivarClubManager.ViewModels.Activities.Page.ItemEdit;

namespace SzivarClubManager.ViewModels.Activities.Page.Wrapper;

[PageActivityCollectionItem(typeof(Shop), "Shops", 1)]
public sealed class ShopActivityViewModel(IPageFactory<Shop> factory) :
    PageActivityViewModel<Shop, ShopPageDataViewModel, ShopAddViewModel, ShopEditViewModel>(factory);