using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models;
using SzivarClubManager.Services;
using SzivarClubManager.SourceGeneration;
using SzivarClubManager.ViewModels.Activities.Page.Data;
using SzivarClubManager.ViewModels.Activities.Page.ItemAdd;
using SzivarClubManager.ViewModels.Activities.Page.ItemEdit;

namespace SzivarClubManager.ViewModels.Activities.Page.Wrapper;

[PageActivityCollectionItem(typeof(User), "Users", 1)]
public sealed class UserActivityViewModel(
    PopupService popupService,
    IPageFactory<User> factory
) : PageActivityViewModel<User, UserFilter, UserPageDataViewModel, UserAddViewModel, UserEditViewModel>(popupService, factory);