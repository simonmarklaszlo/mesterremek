using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;

namespace SzivarClubManager.Datasources.Database.Filters;

[ModelFilter(typeof(Cigar))]
public sealed partial class CigarFilter : IFilter<Cigar>
{
    [ModelFilterProperty("id", "Id")] public int? MinId { get; set; }
    [ModelFilterProperty("id", "Id")] public int? MaxId { get; set; }
    [ModelFilterProperty("name")] public string? Name { get; set; }
}