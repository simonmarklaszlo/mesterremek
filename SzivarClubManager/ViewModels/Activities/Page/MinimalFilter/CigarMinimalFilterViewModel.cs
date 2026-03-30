using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Models;

namespace SzivarClubManager.ViewModels.Activities.Page.MinimalFilter;

public sealed class CigarMinimalFilterViewModel(
    CigarFilter filter,
    IRelayCommand showFilterCommand,
    IRelayCommand triggerSearchCommand
) : MinimalFilterViewModel<CigarFilter, Cigar>(filter, showFilterCommand, triggerSearchCommand);