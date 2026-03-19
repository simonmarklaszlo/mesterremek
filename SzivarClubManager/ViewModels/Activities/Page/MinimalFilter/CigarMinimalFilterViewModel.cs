using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Models;

namespace SzivarClubManager.ViewModels.Activities.Page.MinimalFilter;

public sealed class CigarMinimalFilterViewModel(
    IRelayCommand showFilterCommand,
    IRelayCommand triggerSearchCommand,
    CigarFilter filter
) : MinimalFilterViewModel<CigarFilter, Cigar>(showFilterCommand, triggerSearchCommand, filter);