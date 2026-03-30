using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models;
using SzivarClubManager.Services;
using SzivarClubManager.ViewModels.Activities.Page.Filter;
using SzivarClubManager.ViewModels.Activities.Page.MinimalFilter;

namespace SzivarClubManager.ViewModels.Activities.Page.Data;

public sealed class ShopPageDataViewModel : PageDataViewModel<Shop, ShopFilter>
{
    public override Shop[] CurrentPageData
    {
        get;
        protected set => SetProperty(ref field, value);
    } = [];

    public override Shop[] SelectedItems
    {
        get;
        set
        {
            SetProperty(ref field, value);
            OnSelectedItemsChanged();
        }
    } = [];

    protected override ShopFilter Filter { get; }
    public override MinimalFilterViewModel<ShopFilter, Shop> MinimalFilterViewModel { get; }
    public override FilterViewModel<ShopFilter, Shop> FilterViewModel { get; }



    public ShopPageDataViewModel(PopupService popupService, PageController<Shop> controller, IPageFactory<Shop> factory) : base(popupService, controller, factory)
    {
        Filter = new ShopFilter();
        MinimalFilterViewModel = new ShopMinimalFilterViewModel(Filter, ShowFilterPopupCommand, TriggerSearchCommand);
        FilterViewModel = new ShopFilterViewModel(popupService, Filter, TriggerSearchCommand);
    }
}