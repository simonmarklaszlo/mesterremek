using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models;
using SzivarClubManager.Services;
using SzivarClubManager.SourceGeneration;
using SzivarClubManager.ViewModels.Activities.Page.Data;
using SzivarClubManager.ViewModels.Activities.Page.ItemAdd;
using SzivarClubManager.ViewModels.Activities.Page.ItemEdit;

namespace SzivarClubManager.ViewModels.Activities.Page.Wrapper;

[PageActivityCollectionItem(typeof(Cigar), "Cigars", 1)]
public sealed class CigarActivityViewModel(
    PopupService popupService,
    IPageFactory<Cigar> factory
) : PageActivityViewModel<Cigar, CigarFilter, CigarPageDataViewModel, CigarAddViewModel, CigarEditViewModel>(popupService, factory);