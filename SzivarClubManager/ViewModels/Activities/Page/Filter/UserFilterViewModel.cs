using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;

namespace SzivarClubManager.ViewModels.Activities.Page.Filter;

[ModelFilterViewModel]
public sealed partial class UserFilterViewModel(
    IRelayCommand afterApplyCommand
) : FilterViewModel<UserFilter, User>(afterApplyCommand)
{
    [PredicateOfProperty(nameof(UserFilter.MinId))]
    private static bool MinIdPredicate(int id) => id > 0;

    [PredicateOfProperty(nameof(UserFilter.MaxId))]
    private static bool MaxIdPredicate(int id) => true;

    [PredicateOfProperty(nameof(UserFilter.Name))]
    [PredicateOfProperty(nameof(UserFilter.Email))]
    private static bool StringPredicate(string text) => !text.Contains('"');
}