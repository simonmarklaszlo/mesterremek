using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Models;
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

    public CigarPageDataViewModel()
    {
        Filter = new CigarFilter();
        MinimalFilterViewModel = new CigarMinimalFilterViewModel(ShowFilterPopupCommand, TriggerSearchCommand, Filter);
        FilterViewModel = new CigarFilterViewModel(TriggerSearchCommand)
        {
            Filter = Filter
        };
    }
}