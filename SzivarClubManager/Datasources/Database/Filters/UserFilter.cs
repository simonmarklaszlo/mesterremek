using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;

namespace SzivarClubManager.Datasources.Database.Filters;

[ModelFilter(typeof(User))]
public sealed partial class UserFilter : IFilter<User>
{
    [ModelFilterProperty("id", "Id")] public int? MinId { get; set; }
    [ModelFilterProperty("id", "Id")] public int? MaxId { get; set; }
    [ModelFilterProperty("email")] public string? Email { get; set; }
    [ModelFilterProperty("name")] public string? Name { get; set; }
}