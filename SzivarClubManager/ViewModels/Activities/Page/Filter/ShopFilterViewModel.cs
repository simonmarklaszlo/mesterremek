using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;

namespace SzivarClubManager.ViewModels.Activities.Page.Filter;

[ModelFilterViewModel]
public sealed partial class ShopFilterViewModel(
    IRelayCommand afterApplyCommand
) : FilterViewModel<ShopFilter, Shop>(afterApplyCommand)
{
    [PredicateOfProperty(nameof(ShopFilter.MinId))]
    private static bool MinIdPredicate(int id) => id > 0;

    [PredicateOfProperty(nameof(ShopFilter.MaxId))]
    private static bool MaxIdPredicate(int id) => true;


    [PredicateOfProperty(nameof(ShopFilter.Name))]
    [PredicateOfProperty(nameof(ShopFilter.Address))]
    [PredicateOfProperty(nameof(ShopFilter.City))]
    private static bool StringPredicate(string text) => !text.Contains('"');

    [PredicateOfProperty(nameof(ShopFilter.MinLatitude))]
    [PredicateOfProperty(nameof(ShopFilter.MaxLatitude))]
    private static bool LatitudePredicate(double latitude) => latitude is >= 45.74 and <= 48.59;


    [PredicateOfProperty(nameof(ShopFilter.MinLongitude))]
    [PredicateOfProperty(nameof(ShopFilter.MaxLongitude))]
    private static bool LongitudePredicate(double longitude) => longitude is >= 16.11 and <= 22.9;
}