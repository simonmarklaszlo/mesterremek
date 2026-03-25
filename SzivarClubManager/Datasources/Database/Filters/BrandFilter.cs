using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;
using SzivarClubManager.SourceGeneration.Filter;

namespace SzivarClubManager.Datasources.Database.Filters;

[ModelFilter(typeof(Brand))]
public sealed partial class BrandFilter : IFilter<Brand>
{
    [ModelFilterProperty("id", "Id")] public int? MinId { get; set; }
    [ModelFilterProperty("id", "Id")] public int? MaxId { get; set; }
    [ModelFilterProperty("name")] public string? Name { get; set; }
}