using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models;
using SzivarClubManager.Services;
using SzivarClubManager.SourceGeneration.Activity;
using SzivarClubManager.ViewModels.Activities.Page.Data;
using SzivarClubManager.ViewModels.Activities.Page.ItemAdd;
using SzivarClubManager.ViewModels.Activities.Page.ItemEdit;

namespace SzivarClubManager.ViewModels.Activities.Page.Wrapper;

[PageActivityCollectionItem("Szivarok", typeof(Cigar), 1)]
public sealed class CigarActivityViewModel(
    PopupService popupService,
    IPageFactory<Cigar> factory
) : PageActivityViewModel<Cigar, CigarFilter, CigarPageDataViewModel, CigarAddViewModel, CigarEditViewModel>(popupService)
{
    protected override CigarPageDataViewModel DataViewModel => field ??= new CigarPageDataViewModel(PopupService, Controller, factory);
    protected override CigarAddViewModel ItemAddViewModel => field ??= new CigarAddViewModel(PopupService);
    protected override CigarEditViewModel ItemEditViewModel => field ??= new CigarEditViewModel(Controller);
    protected override CigarFilter Filter => field ??= new CigarFilter();
}