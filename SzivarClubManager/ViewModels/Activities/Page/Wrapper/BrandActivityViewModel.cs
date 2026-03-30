using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models;
using SzivarClubManager.Services;
using SzivarClubManager.SourceGeneration.Activity;
using SzivarClubManager.ViewModels.Activities.Page.Data;
using SzivarClubManager.ViewModels.Activities.Page.ItemAdd;
using SzivarClubManager.ViewModels.Activities.Page.ItemEdit;

namespace SzivarClubManager.ViewModels.Activities.Page.Wrapper;

[PageActivityCollectionItem("Brands", typeof(Brand), 1)]
public sealed class BrandActivityViewModel(
    PopupService popupService,
    IPageFactory<Brand> factory
) : PageActivityViewModel<Brand, BrandFilter, BrandPageDataViewModel, BrandAddViewModel, BrandEditViewModel>(popupService, factory);