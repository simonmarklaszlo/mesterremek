using System.Threading.Tasks;
using SzivarClubManager.Datasources;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;
using SzivarClubManager.ViewModels.Activities.Page.Data;
using SzivarClubManager.ViewModels.Activities.Page.ItemAdd;
using SzivarClubManager.ViewModels.Activities.Page.ItemEdit;
using SzivarClubManager.ViewModels.Activities.Page.Navigation;

namespace SzivarClubManager.ViewModels.Activities.Page.Wrapper;

[PageActivityCollectionItem(typeof(Cigar))]
public sealed class CigarActivityViewModel : PageActivityViewModel
{
    private readonly CigarPageDataViewModel _pageDataViewModel;
    private CigarEditViewModel? _itemEditViewModel;
    private CigarAddViewModel? _itemAddViewModel;

    private readonly PageController<Cigar> _controller;


    public CigarActivityViewModel(IPageFactory<Cigar> factory)
    {
        _controller = new PageController<Cigar>();
        _controller.ItemSelected += ControllerOnItemSelected;
        _controller.ItemEdited += ControllerOnItemEdited;
        _controller.ItemAdded += ControllerOnItemAdded;
        _controller.BackNavigated += ControllerOnBackNavigated;

        _pageDataViewModel = new CigarPageDataViewModel(factory, _controller);

        ViewModel = _pageDataViewModel;
    }


    public override async Task InitializeAsync()
    {
        if (IsInitialized) return;
        if (ViewModel is CigarPageDataViewModel pdvm) await pdvm.InitializeAsync();
    }

    public override void OnOpening()
    {
        if (ViewModel is not CigarPageDataViewModel)
        {
            ViewModel = _pageDataViewModel;
        }
    }


    private void ControllerOnBackNavigated() => ViewModel = _pageDataViewModel;

    private void ControllerOnItemSelected(Cigar item) => OpenItem(item, false);
    private void ControllerOnItemEdited(Cigar item) => OpenItem(item, true);
    private void ControllerOnItemAdded() => ViewModel = _itemAddViewModel ??= new CigarAddViewModel(_controller);
    private void OpenItem(Cigar item, bool isEdit)
    {
        if (_itemEditViewModel is null) _itemEditViewModel = new CigarEditViewModel(_controller, item);
        else _itemEditViewModel.SourceItem = item;

        _itemEditViewModel.IsEdit = isEdit;

        ViewModel = _itemEditViewModel;
    }
}