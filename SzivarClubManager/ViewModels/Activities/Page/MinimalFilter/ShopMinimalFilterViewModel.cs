using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Models;

namespace SzivarClubManager.ViewModels.Activities.Page.MinimalFilter;

public sealed class ShopMinimalFilterViewModel(
    IRelayCommand showFilterCommand,
    IRelayCommand triggerSearchCommand,
    ShopFilter filter
) : MinimalFilterViewModel<ShopFilter, Shop>(showFilterCommand, triggerSearchCommand, filter);