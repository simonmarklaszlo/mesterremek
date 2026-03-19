using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Models;
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


    public ShopPageDataViewModel()
    {
        Filter = new ShopFilter();
        MinimalFilterViewModel = new ShopMinimalFilterViewModel(ShowFilterPopupCommand, TriggerSearchCommand, Filter);
        FilterViewModel = new ShopFilterViewModel(TriggerSearchCommand)
        {
            Filter = Filter
        };
    }
}