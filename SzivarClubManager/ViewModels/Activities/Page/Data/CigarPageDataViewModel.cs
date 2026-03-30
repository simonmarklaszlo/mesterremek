using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models;
using SzivarClubManager.Services;
using SzivarClubManager.ViewModels.Activities.Page.Filter;
using SzivarClubManager.ViewModels.Activities.Page.MinimalFilter;

namespace SzivarClubManager.ViewModels.Activities.Page.Data;

public sealed class CigarPageDataViewModel : PageDataViewModel<Cigar, CigarFilter>
{
    public override Cigar[] CurrentPageData
    {
        get;
        protected set => SetProperty(ref field, value);
    } = [];

    public override Cigar[] SelectedItems
    {
        get;
        set
        {
            SetProperty(ref field, value);
            OnSelectedItemsChanged();
        }
    } = [];

    protected override CigarFilter Filter { get; }
    public override MinimalFilterViewModel<CigarFilter, Cigar> MinimalFilterViewModel { get; }
    public override FilterViewModel<CigarFilter, Cigar> FilterViewModel { get; }


    public CigarPageDataViewModel(PopupService popupService, PageController<Cigar> controller, IPageFactory<Cigar> factory) : base(popupService, controller, factory)
    {
        Filter = new CigarFilter();
        MinimalFilterViewModel = new CigarMinimalFilterViewModel(Filter, ShowFilterPopupCommand, TriggerSearchCommand);
        FilterViewModel = new CigarFilterViewModel(popupService, Filter, TriggerSearchCommand);
    }
}