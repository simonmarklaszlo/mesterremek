using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Models;
using SzivarClubManager.Services;
using SzivarClubManager.SourceGeneration.Filter;

namespace SzivarClubManager.ViewModels.Activities.Page.Filter;

[ModelFilterViewModel]
public sealed partial class CigarFilterViewModel(
    PopupService popupService,
    CigarFilter filter,
    IRelayCommand afterApplyCommand
) : FilterViewModel<CigarFilter, Cigar>(popupService, filter, afterApplyCommand)
{
    [PredicateOfProperty(nameof(CigarFilter.MinId))]
    private static bool MinIdPredicate(int id) => id > 0;

    [PredicateOfProperty(nameof(CigarFilter.MaxId))]
    private static bool MaxIdPredicate(int id) => true;

    [PredicateOfProperty(nameof(CigarFilter.Name))]
    private static bool StringPredicate(string text) => !text.Contains('"');
}