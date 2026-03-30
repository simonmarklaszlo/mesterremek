using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Models;

namespace SzivarClubManager.ViewModels.Activities.Page.MinimalFilter;

public sealed class BrandMinimalFilterViewModel(
    BrandFilter filter,
    IRelayCommand showFilterCommand,
    IRelayCommand triggerSearchCommand
) : MinimalFilterViewModel<BrandFilter, Brand>(filter, showFilterCommand, triggerSearchCommand);