using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models;
using SzivarClubManager.Services;
using SzivarClubManager.ViewModels.Activities.Page.Filter;
using SzivarClubManager.ViewModels.Activities.Page.MinimalFilter;

namespace SzivarClubManager.ViewModels.Activities.Page.Data;

public sealed class UserPageDataViewModel : PageDataViewModel<User, UserFilter>
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

    protected override UserFilter Filter { get; }
    public override MinimalFilterViewModel<UserFilter, User> MinimalFilterViewModel { get; }
    public override FilterViewModel<UserFilter, User> FilterViewModel { get; }

    public UserPageDataViewModel(PopupService popupService, PageController<User> controller, IPageFactory<User> factory) : base(popupService, controller, factory)
    {
        Filter = new UserFilter();
        MinimalFilterViewModel = new UserMinimalFilterViewModel(Filter, ShowFilterPopupCommand, TriggerSearchCommand);
        FilterViewModel = new UserFilterViewModel(popupService, Filter, TriggerSearchCommand);
    }
}