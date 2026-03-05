using SzivarClubManager.Models;

namespace SzivarClubManager.ViewModels.Activities.Page.Data;

public sealed class BrandPageDataViewModel : PageDataViewModel<Brand>
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
}