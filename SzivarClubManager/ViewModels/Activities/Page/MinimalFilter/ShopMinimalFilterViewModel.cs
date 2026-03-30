using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Models;

namespace SzivarClubManager.ViewModels.Activities.Page.MinimalFilter;

public sealed class ShopMinimalFilterViewModel(
    ShopFilter filter,
    IRelayCommand showFilterCommand,
    IRelayCommand triggerSearchCommand
) : MinimalFilterViewModel<ShopFilter, Shop>(filter, showFilterCommand, triggerSearchCommand);