using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Models;
using SzivarClubManager.ViewModels.Activities.Page.Filter;
using SzivarClubManager.ViewModels.Activities.Page.MinimalFilter;

namespace SzivarClubManager.ViewModels.Activities.Page.Data;

public sealed class BrandPageDataViewModel : PageDataViewModel<Brand, BrandFilter>
{
    public override Brand[] CurrentPageData
    {
        get;
        protected set => SetProperty(ref field, value);
    } = [];

    public override Brand[] SelectedItems
    {
        get;
        set
        {
            SetProperty(ref field, value);
            OnSelectedItemsChanged();
        }
    } = [];

    protected override BrandFilter Filter { get; }
    public override MinimalFilterViewModel<BrandFilter, Brand> MinimalFilterViewModel { get; }
    public override FilterViewModel<BrandFilter, Brand> FilterViewModel { get; }

    public BrandPageDataViewModel()
    {
        Filter = new BrandFilter();
        MinimalFilterViewModel = new BrandMinimalFilterViewModel(ShowFilterPopupCommand, TriggerSearchCommand, Filter);
        FilterViewModel = new BrandFilterViewModel(TriggerSearchCommand)
        {
            Filter = Filter
        };
    }
}