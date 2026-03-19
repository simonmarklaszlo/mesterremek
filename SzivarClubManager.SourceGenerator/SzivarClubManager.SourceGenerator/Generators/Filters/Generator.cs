using Microsoft.CodeAnalysis;
using SzivarClubManager.SourceGenerator.Generators.Filters.Generation;
using SzivarClubManager.SourceGenerator.Generators.Filters.Target;

namespace SzivarClubManager.SourceGenerator.Generators.Filters;

[Generator]
public class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var filters = FilterModel.GetCandidates(context);

        context.RegisterSourceOutput(filters, (ctx, model) =>
        {
            ctx.AddSource(ModelFilter.GetFileName(model), ModelFilter.GenerateSource(model));
        });
    }
}