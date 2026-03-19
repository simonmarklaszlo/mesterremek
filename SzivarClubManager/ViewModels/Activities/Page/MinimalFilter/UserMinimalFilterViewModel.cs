using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Models;

namespace SzivarClubManager.ViewModels.Activities.Page.MinimalFilter;

public sealed class UserMinimalFilterViewModel(
    IRelayCommand showFilterCommand,
    IRelayCommand triggerSearchCommand,
    UserFilter filter
) : MinimalFilterViewModel<UserFilter, User>(showFilterCommand, triggerSearchCommand, filter);