using System.Threading.Tasks;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Models;
using SzivarClubManager.Services;
using SzivarClubManager.ViewModels.Activities.Page.Data;
using SzivarClubManager.ViewModels.Activities.Page.ItemAdd;
using SzivarClubManager.ViewModels.Activities.Page.ItemEdit;

namespace SzivarClubManager.ViewModels.Activities.Page.Wrapper;

public abstract class PageActivityViewModel<TModel, TFilter, TDataViewModel, TAddViewModel, TEditViewModel> : PageActivityViewModel
    where TModel : class, IModel
    where TFilter : IFilter<TModel>
    where TDataViewModel : PageDataViewModel<TModel, TFilter>
    where TAddViewModel : ItemAddViewModel
    where TEditViewModel : ItemEditViewModel<TModel>
{
    protected abstract TDataViewModel DataViewModel { get; }
    protected abstract TAddViewModel ItemAddViewModel { get; }
    protected abstract TEditViewModel ItemEditViewModel { get; }
    protected abstract TFilter Filter { get; }

    protected PageController<TModel> Controller { get; }

    protected PageActivityViewModel(PopupService popupService) : base(popupService)
    {
        Controller = new PageController<TModel>();
        Controller.AddPopupRequested += ControllerOnAddPopupRequested;
        Controller.ViewPopupRequested += ControllerOnViewPopupRequested;
        Controller.EditPopupRequested += ControllerOnEditPopupRequested;
        Controller.PopupCloseRequested += () => PopupService.ClosePopup();
    }


    public override async Task InitializeAsync()
    {
        if (IsInitialized) return;

        ViewModel ??= DataViewModel;
        if (ViewModel is TDataViewModel dataVm) await dataVm.InitializeAsync();

        await base.InitializeAsync();
    }

    private void ControllerOnEditPopupRequested(TModel item)
    {
        ItemEditViewModel.SourceItem = item;
        ItemEditViewModel.IsEdit = true;
        PopupService.ShowPopup(ItemEditViewModel);
    }

    private void ControllerOnViewPopupRequested(TModel item)
    {
        ItemEditViewModel.SourceItem = item;
        ItemEditViewModel.IsEdit = false;
        PopupService.ShowPopup(ItemEditViewModel);
    }

    private void ControllerOnAddPopupRequested()
    {
        PopupService.ShowPopup(ItemAddViewModel);
    }
}