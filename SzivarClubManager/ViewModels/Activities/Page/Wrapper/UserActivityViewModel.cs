using System.Threading.Tasks;
using SzivarClubManager.Datasources;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;
using SzivarClubManager.ViewModels.Activities.Page.Data;
using SzivarClubManager.ViewModels.Activities.Page.ItemEdit;
using SzivarClubManager.ViewModels.Activities.Page.Navigation;

namespace SzivarClubManager.ViewModels.Activities.Page.Wrapper;

[PageActivityCollectionItem(typeof(User))]
public sealed class UserActivityViewModel : PageActivityViewModel
{
    private readonly UserPageDataViewModel _pageDataViewModel;
    private UserEditViewModel? _itemEditViewModel;

    private readonly PageController<User> _controller;


    public UserActivityViewModel(IPageFactory<User> factory)
    {
        _controller = new PageController<User>();
        _controller.ItemSelected += ControllerOnItemSelected;
        _controller.BackNavigated += ControllerOnBackNavigated;

        _pageDataViewModel = new UserPageDataViewModel(factory, _controller);

        ViewModel = _pageDataViewModel;
    }


    public override async Task InitializeAsync()
    {
        if (IsInitialized) return;
        if (ViewModel is UserPageDataViewModel pdvm) await pdvm.InitializeAsync();
    }

    public override void OnOpening()
    {
        if (ViewModel is not UserPageDataViewModel)
        {
            ViewModel = _pageDataViewModel;
        }
    }

    private void ControllerOnBackNavigated() => ViewModel = _pageDataViewModel;

    private void ControllerOnItemSelected(User item, bool isEdit)
    {
        if (_itemEditViewModel is null) _itemEditViewModel = new UserEditViewModel(_controller, item);
        else _itemEditViewModel.SourceItem = item;

        _itemEditViewModel.IsEdit = isEdit;

        ViewModel = _itemEditViewModel;
    }
}