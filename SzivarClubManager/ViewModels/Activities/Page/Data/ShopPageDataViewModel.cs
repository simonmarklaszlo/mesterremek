using SzivarClubManager.Datasources;
using SzivarClubManager.Models;
using SzivarClubManager.ViewModels.Activities.Page.Navigation;

namespace SzivarClubManager.ViewModels.Activities.Page.Data;

public sealed class ShopPageDataViewModel : PageDataViewModel<Shop>
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

    public ShopPageDataViewModel(IPageFactory<Shop> factory, PageController<Shop> controller) : base(factory, controller) { }
}