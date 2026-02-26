using SzivarClubManager.Datasources;
using SzivarClubManager.Models;
using SzivarClubManager.ViewModels.Activities.Page.Navigation;

namespace SzivarClubManager.ViewModels.Activities.Page.Data;

public sealed class UserPageDataViewModel : PageDataViewModel<User>
{
    public override User[] CurrentPageData
    {
        get;
        protected set => SetProperty(ref field, value);
    } = [];

    public override User[] SelectedItems
    {
        get;
        set
        {
            SetProperty(ref field, value);
            OnSelectedItemsChanged();
        }
    } = [];

    public UserPageDataViewModel(IPageFactory<User> factory, PageController<User> controller) : base(factory, controller) { }
}