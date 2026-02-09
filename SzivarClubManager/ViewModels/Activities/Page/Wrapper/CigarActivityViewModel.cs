using System.Threading.Tasks;
using SzivarClubManager.Datasources;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;
using SzivarClubManager.ViewModels.Activities.Page.Data;
using SzivarClubManager.ViewModels.Activities.Page.ItemEdit;
using SzivarClubManager.ViewModels.Activities.Page.Navigation;

namespace SzivarClubManager.ViewModels.Activities.Page.Wrapper;

[PageActivityCollectionItem(typeof(Cigar))]
public sealed class CigarActivityViewModel : PageActivityViewModel
{
    private readonly CigarPageDataViewModel _pageDataViewModel;
    private CigarEditViewModel? _itemEditViewModel;

    private readonly PageController<Cigar> _controller;


    public CigarActivityViewModel(IPageFactory<Cigar> factory)
    {
        _controller = new PageController<Cigar>();
        _controller.ItemSelected += ControllerOnItemSelected;
        _controller.BackNavigated += ControllerOnBackNavigated;

        var pdvm = new CigarPageDataViewModel(factory, _controller);
        _pageDataViewModel = pdvm;
        ViewModel = pdvm;
    }


    public override async Task InitializeAsync()
    {
        if (IsInitialized) return;
        if (ViewModel is CigarPageDataViewModel pdvm) await pdvm.InitializeAsync();
    }

    private void ControllerOnBackNavigated() => ViewModel = _pageDataViewModel;

    private void ControllerOnItemSelected(Cigar item, NavigationIntent intent)
    {
        if (_itemEditViewModel is null) _itemEditViewModel = new CigarEditViewModel(item, _controller);
        else _itemEditViewModel.SelectedItem = item;

        _itemEditViewModel.SetIntent(intent);

        ViewModel = _itemEditViewModel;
    }
}