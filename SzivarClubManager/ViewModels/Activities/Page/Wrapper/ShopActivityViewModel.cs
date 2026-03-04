using System.Threading.Tasks;
using SzivarClubManager.Datasources;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;
using SzivarClubManager.ViewModels.Activities.Page.Data;
using SzivarClubManager.ViewModels.Activities.Page.ItemAdd;
using SzivarClubManager.ViewModels.Activities.Page.ItemEdit;
using SzivarClubManager.ViewModels.Activities.Page.Navigation;

namespace SzivarClubManager.ViewModels.Activities.Page.Wrapper;

[PageActivityCollectionItem(typeof(Shop))]
public sealed class ShopActivityViewModel : PageActivityViewModel
{
    private readonly ShopPageDataViewModel _pageDataViewModel;
    private ShopEditViewModel? _itemEditViewModel;
    private ShopAddViewModel? _itemAddViewModel;

    private readonly PageController<Shop> _controller;


    public ShopActivityViewModel(IPageFactory<Shop> factory)
    {
        _controller = new PageController<Shop>();
        _controller.ItemSelected += ControllerOnItemSelected;
        _controller.ItemEdited += ControllerOnItemEdited;
        _controller.ItemAdded += ControllerOnItemAdded;
        _controller.BackNavigated += ControllerOnBackNavigated;

        _pageDataViewModel = new ShopPageDataViewModel(factory, _controller);

        ViewModel = _pageDataViewModel;
    }


    public override async Task InitializeAsync()
    {
        if (IsInitialized) return;
        if (ViewModel is ShopPageDataViewModel pdvm) await pdvm.InitializeAsync();
    }

    public override void OnOpening()
    {
        if (ViewModel is not ShopPageDataViewModel)
        {
            ViewModel = _pageDataViewModel;
        }
    }

    private void ControllerOnBackNavigated() => ViewModel = _pageDataViewModel;

    private void ControllerOnItemSelected(Shop item) => OpenItem(item, false);
    private void ControllerOnItemEdited(Shop item) => OpenItem(item, true);
    private void ControllerOnItemAdded() => ViewModel = _itemAddViewModel ??= new ShopAddViewModel(_controller);

    private void OpenItem(Shop item, bool isEdit)
    {
        if (_itemEditViewModel is null) _itemEditViewModel = new ShopEditViewModel(_controller, item);
        else _itemEditViewModel.SourceItem = item;

        _itemEditViewModel.IsEdit = isEdit;

        ViewModel = _itemEditViewModel;
    }
}