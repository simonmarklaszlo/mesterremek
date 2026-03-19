using System.Linq;
using Microsoft.CodeAnalysis;
using SzivarClubManager.SourceGenerator.Generators.Filters.Target;
using SzivarClubManager.SourceGenerator.Generators.FilterViewModels.Generation;
using SzivarClubManager.SourceGenerator.Generators.FilterViewModels.Target;

namespace SzivarClubManager.SourceGenerator.Generators.FilterViewModels;

[Generator]
public class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var vms = FilterViewModel.GetCandidates(context);
        var filters = FilterModel.GetCandidates(context).Collect();


        var matched = vms.Combine(filters)
            .Select(static (pair, _) =>
            {
                var (vm, filtersList) = pair;

                var match = filtersList.FirstOrDefault(f =>
                    SymbolEqualityComparer.Default.Equals(f.FilterType, vm.FilterSymbol));

                return (vm, match);
            });

        context.RegisterSourceOutput(matched, (ctx, tuple) =>
        {
            var (vm, match) = tuple;
            if (vm is null || match is null) return;
            ctx.AddSource(ModelFilterViewModel.GetFileName(vm), ModelFilterViewModel.GenerateSource(vm, match));
        });
    }
}