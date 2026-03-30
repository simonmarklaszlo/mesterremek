using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Models;

namespace SzivarClubManager.ViewModels.Activities.Page.MinimalFilter;

public sealed class UserMinimalFilterViewModel(
    UserFilter filter,
    IRelayCommand showFilterCommand,
    IRelayCommand triggerSearchCommand
) : MinimalFilterViewModel<UserFilter, User>(filter, showFilterCommand, triggerSearchCommand);