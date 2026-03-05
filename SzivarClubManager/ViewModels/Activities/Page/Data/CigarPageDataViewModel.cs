using SzivarClubManager.Models;

namespace SzivarClubManager.ViewModels.Activities.Page.Data;

public sealed class CigarPageDataViewModel : PageDataViewModel<Cigar>
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
}