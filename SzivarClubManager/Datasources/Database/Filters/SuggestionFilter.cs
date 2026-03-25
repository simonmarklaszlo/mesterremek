using SzivarClubManager.Models.Suggestions;
using SzivarClubManager.SourceGeneration;
using SzivarClubManager.SourceGeneration.Filter;

namespace SzivarClubManager.Datasources.Database.Filters;

[ModelFilter(typeof(Suggestion))]
public sealed partial class SuggestionFilter : IFilter<Suggestion>
{
    [ModelFilterProperty("id", "Id")]
    public int? MinId { get; set; }

    [ModelFilterProperty("id", "Id")]
    public int? MaxId { get; set; }

    [ModelFilterProperty("type_id", "TypeId")]
    public int? MinTypeId { get; set; }

    [ModelFilterProperty("type_id", "TypeId")]
    public int? MaxTypeId { get; set; }

    [ModelFilterProperty("shop_id", "ShopId")]
    public int? MinShopId { get; set; }

    [ModelFilterProperty("shop_id", "ShopId")]
    public int? MaxShopId { get; set; }

    [ModelFilterProperty("proposed_value")]
    public string? ProposedValue { get; set; }

    [ModelFilterProperty("additional_data")]
    public string? AdditionalData { get; set; }

    [ModelFilterProperty("user_id", "UserId")]
    public int? MinUserId { get; set; }

    [ModelFilterProperty("user_id", "UserId")]
    public int? MaxUserId { get; set; }

    [ModelFilterProperty("status_id", "StatusId")]
    public int? MinStatusId { get; set; }

    [ModelFilterProperty("status_id", "StatusId")]
    public int? MaxStatusId { get; set; }
}