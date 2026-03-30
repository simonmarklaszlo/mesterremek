using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Models;
using SzivarClubManager.Services;
using SzivarClubManager.SourceGeneration.Filter;

namespace SzivarClubManager.ViewModels.Activities.Page.Filter;

[ModelFilterViewModel]
public sealed partial class BrandFilterViewModel(
    PopupService popupService,
    BrandFilter filter,
    IRelayCommand afterApplyCommand
) : FilterViewModel<BrandFilter, Brand>(popupService, filter, afterApplyCommand)
{
    [PredicateOfProperty(nameof(BrandFilter.MinId))]
    private static bool MinIdPredicate(int id) => id > 0;

    [PredicateOfProperty(nameof(BrandFilter.MaxId))]
    private static bool MaxIdPredicate(int id) => true;

    [PredicateOfProperty(nameof(BrandFilter.Name))]
    private static bool StringPredicate(string text) => !text.Contains('"');
}