using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Models;

namespace SzivarClubManager.ViewModels.Activities.Page.MinimalFilter;

public sealed class BrandMinimalFilterViewModel(
    IRelayCommand showFilterCommand,
    IRelayCommand triggerSearchCommand,
    BrandFilter filter
) : MinimalFilterViewModel<BrandFilter, Brand>(showFilterCommand, triggerSearchCommand, filter);