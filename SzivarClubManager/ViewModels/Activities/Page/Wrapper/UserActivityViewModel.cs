using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models;
using SzivarClubManager.Services;
using SzivarClubManager.SourceGeneration.Activity;
using SzivarClubManager.ViewModels.Activities.Page.Data;
using SzivarClubManager.ViewModels.Activities.Page.ItemAdd;
using SzivarClubManager.ViewModels.Activities.Page.ItemEdit;

namespace SzivarClubManager.ViewModels.Activities.Page.Wrapper;

[PageActivityCollectionItem("Felhasználók", typeof(User), 1)]
public sealed class UserActivityViewModel(
    PopupService popupService,
    IPageFactory<User> factory
) : PageActivityViewModel<User, UserFilter, UserPageDataViewModel, UserAddViewModel, UserEditViewModel>(popupService)
{
    protected override UserPageDataViewModel DataViewModel => field ??= new UserPageDataViewModel(PopupService, Controller, factory);
    protected override UserAddViewModel ItemAddViewModel => field ??= new UserAddViewModel(PopupService);
    protected override UserEditViewModel ItemEditViewModel => field ??= new UserEditViewModel(Controller);
    protected override UserFilter Filter => field ??= new UserFilter();
}