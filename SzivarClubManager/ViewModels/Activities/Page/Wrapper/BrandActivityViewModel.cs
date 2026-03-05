using SzivarClubManager.Datasources;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;
using SzivarClubManager.ViewModels.Activities.Page.Data;
using SzivarClubManager.ViewModels.Activities.Page.ItemAdd;
using SzivarClubManager.ViewModels.Activities.Page.ItemEdit;

namespace SzivarClubManager.ViewModels.Activities.Page.Wrapper;

[PageActivityCollectionItem(typeof(Brand), "Brands", 1)]
public sealed class BrandActivityViewModel(IPageFactory<Brand> factory) :
    PageActivityViewModel<Brand, BrandPageDataViewModel, BrandAddViewModel, BrandEditViewModel>(factory);
