using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;
using SzivarClubManager.SourceGeneration.Filter;

namespace SzivarClubManager.Datasources.Database.Filters;

[ModelFilter(typeof(Shop))]
public sealed partial class ShopFilter : IFilter<Shop>
{
    [ModelFilterProperty("id", "Id")]
    public int? MinId { get; set; }

    [ModelFilterProperty("id", "Id")]
    public int? MaxId { get; set; }

    [ModelFilterProperty("name")]
    public string? Name { get; set; }

    [ModelFilterProperty("address")]
    public string? Address { get; set; }

    [ModelFilterProperty("city")]
    public string? City { get; set; }

    [ModelFilterProperty("ST_Y(location)", "Latitude")]
    public double? MinLatitude { get; set; }

    [ModelFilterProperty("ST_Y(location)", "Latitude")]
    public double? MaxLatitude { get; set; }

    [ModelFilterProperty("ST_X(location)", "Longitude")]
    public double? MinLongitude { get; set; }

    [ModelFilterProperty("ST_X(location)", "Longitude")]
    public double? MaxLongitude { get; set; }
}