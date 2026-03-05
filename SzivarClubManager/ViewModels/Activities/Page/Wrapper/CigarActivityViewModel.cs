using SzivarClubManager.Datasources;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;
using SzivarClubManager.ViewModels.Activities.Page.Data;
using SzivarClubManager.ViewModels.Activities.Page.ItemAdd;
using SzivarClubManager.ViewModels.Activities.Page.ItemEdit;

namespace SzivarClubManager.ViewModels.Activities.Page.Wrapper;

[PageActivityCollectionItem(typeof(Cigar), "Cigars", 1)]
public sealed class CigarActivityViewModel(IPageFactory<Cigar> factory) :
    PageActivityViewModel<Cigar, CigarPageDataViewModel, CigarAddViewModel, CigarEditViewModel>(factory);