using System.Threading.Tasks;
using SzivarClubManager.Datasources;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;
using SzivarClubManager.ViewModels.Activities.Page.Data;
using SzivarClubManager.ViewModels.Activities.Page.ItemEdit;
using SzivarClubManager.ViewModels.Activities.Page.Navigation;

namespace SzivarClubManager.ViewModels.Activities.Page.Wrapper;

[PageActivityCollectionItem(typeof(Shop))]
public sealed class ShopActivityViewModel : PageActivityViewModel
{
    private readonly ShopPageDataViewModel _pageDataViewModel;
    private ShopEditViewModel? _itemEditViewModel;

    private readonly PageController<Shop> _controller;


    public ShopActivityViewModel(IPageFactory<Shop> factory)
    {
        _controller = new PageController<Shop>();
        _controller.ItemSelected += ControllerOnItemSelected;
        _controller.BackNavigated += ControllerOnBackNavigated;

        var pdvm = new ShopPageDataViewModel(factory, _controller);
        _pageDataViewModel = pdvm;
        ViewModel = pdvm;
    }


    public override async Task InitializeAsync()
    {
        if (IsInitialized) return;
        if (ViewModel is ShopPageDataViewModel pdvm) await pdvm.InitializeAsync();
    }

    private void ControllerOnBackNavigated() => ViewModel = _pageDataViewModel;

    private void ControllerOnItemSelected(Shop item, NavigationIntent intent)
    {
        if (_itemEditViewModel is null) _itemEditViewModel = new ShopEditViewModel(item, _controller);
        else _itemEditViewModel.SelectedItem = item;

        _itemEditViewModel.SetIntent(intent);

        ViewModel = _itemEditViewModel;
    }
}