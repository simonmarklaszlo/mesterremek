using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models;
using SzivarClubManager.Services;
using SzivarClubManager.SourceGeneration.Activity;
using SzivarClubManager.ViewModels.Activities.Page.Data;
using SzivarClubManager.ViewModels.Activities.Page.ItemAdd;
using SzivarClubManager.ViewModels.Activities.Page.ItemEdit;

namespace SzivarClubManager.ViewModels.Activities.Page.Wrapper;

[PageActivityCollectionItem("Márkák", typeof(Brand), 1)]
public sealed class BrandActivityViewModel(
    PopupService popupService,
    IPageFactory<Brand> factory
) : PageActivityViewModel<Brand, BrandFilter, BrandPageDataViewModel, BrandAddViewModel, BrandEditViewModel>(popupService)
{
    protected override BrandPageDataViewModel DataViewModel => field ??= new BrandPageDataViewModel(PopupService, Controller, factory);
    protected override BrandAddViewModel ItemAddViewModel => field ??= new BrandAddViewModel(PopupService);
    protected override BrandEditViewModel ItemEditViewModel => field ??= new BrandEditViewModel(Controller);
    protected override BrandFilter Filter => field ??= new BrandFilter();
}