using System.Threading.Tasks;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models;
using SzivarClubManager.Services;
using SzivarClubManager.ViewModels.Activities.Page.Data;
using SzivarClubManager.ViewModels.Activities.Page.ItemAdd;
using SzivarClubManager.ViewModels.Activities.Page.ItemEdit;
using SzivarClubManager.ViewModels.Activities.Page.Navigation;

namespace SzivarClubManager.ViewModels.Activities.Page.Wrapper;

public abstract class PageActivityViewModel<TModel, TFilter, TDataViewModel, TAddViewModel, TEditViewModel> : PageActivityViewModel
    where TModel : class, IModel
    where TFilter : IFilter<TModel>, new()
    where TDataViewModel : PageDataViewModel<TModel, TFilter>, new()
    where TAddViewModel : ItemAddViewModel<TModel>, new()
    where TEditViewModel : ItemEditViewModel<TModel>, new()
{
    private readonly TDataViewModel _pageDataViewModel;
    private TEditViewModel? _itemEditViewModel;
    private TAddViewModel? _itemAddViewModel;

    private readonly PageController<TModel> _controller;


    protected PageActivityViewModel(PopupService popupService, IPageFactory<TModel> factory, bool itemAddSupported = true, bool itemEditSupported = true) : base(popupService)
    {
        _controller = new PageController<TModel>();
        _controller.ItemSelected += ControllerOnItemSelected;
        if (itemEditSupported) _controller.ItemEdited += ControllerOnItemEdited;
        if (itemAddSupported) _controller.ItemAdded += ControllerOnItemAdded;
        _controller.BackNavigated += ControllerOnBackNavigated;

        _pageDataViewModel = new TDataViewModel
        {
            Factory = factory,
            Controller = _controller,
            PopupService = popupService
        };

        ViewModel = _pageDataViewModel;
    }


    public override async Task InitializeAsync()
    {
        if (IsInitialized) return;
        if (ViewModel is TDataViewModel pdvm) await pdvm.InitializeAsync();
    }

    public override void OnOpening()
    {
        if (ViewModel is not TDataViewModel)
        {
            ViewModel = _pageDataViewModel;
        }
    }


    private void ControllerOnBackNavigated() => ViewModel = _pageDataViewModel;

    private void ControllerOnItemSelected(TModel item) => OpenItem(item, false);
    private void ControllerOnItemEdited(TModel item) => OpenItem(item, true);

    private void ControllerOnItemAdded() => ViewModel = _itemAddViewModel ??= new TAddViewModel { Controller = _controller };

    private void OpenItem(TModel item, bool isEdit)
    {
        if (_itemEditViewModel is null) _itemEditViewModel = new TEditViewModel { Controller = _controller, SourceItem = item };
        else _itemEditViewModel.SourceItem = item;

        _itemEditViewModel.IsEdit = isEdit;

        ViewModel = _itemEditViewModel;
    }
}