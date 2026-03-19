using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using SzivarClubManager.SourceGenerator.Generators.Changes.Generation;
using SzivarClubManager.SourceGenerator.Generators.Changes.Target;
using SzivarClubManager.SourceGenerator.Targets;
using SzivarClubManager.SourceGenerator.Targets.Factory;

namespace SzivarClubManager.SourceGenerator.Generators.Changes;

[Generator]
public class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var models = Model.GetCandidates(context);
        var factories = Factory.GetCandidates(context);

        var paVms = PageActivityViewModel.GetCandidates(context);

        var factoriesCombined = models.Collect()
            .Combine(factories.Collect());

        context.RegisterSourceOutput(factoriesCombined, RegisterFactories);
        context.RegisterSourceOutput(paVms.Collect(), RegisterChangesVm);
    }

    private static void RegisterChangesVm(SourceProductionContext spc, ImmutableArray<PageActivityViewModel> source)
    {
        spc.AddSource(ChangesActivityViewModel.FileName, ChangesActivityViewModel.GenerateSource(source));
    }

    private static void RegisterFactories(SourceProductionContext spc, (ImmutableArray<Model> Left, ImmutableArray<Factory> Right) source)
    {
        var (modelArray, factoryArray) = source;


        var factoryLookup = factoryArray.GroupBy(f => f.ModelSymbol, SymbolEqualityComparer.Default)
            .ToDictionary(g => g.Key, g => g.First(), SymbolEqualityComparer.Default);

        ModelGroup[] paired = new ModelGroup[modelArray.Length];

        for (var i = 0; i < modelArray.Length; i++)
        {
            var model = modelArray[i];
            if (!factoryLookup.TryGetValue(model.ModelSymbol, out var factory))
            {
                spc.ReportDiagnostic(Diagnostic.Create(Warnings.NoFactoryWarning, model.ModelSymbol.Locations.FirstOrDefault(), model.ModelSymbol.Name));
            }

            paired[i] = new ModelGroup(model, factory);
        }

        spc.AddSource(Generation.Changes.FileName, Generation.Changes.GenerateSource(paired));
        spc.AddSource(ModelRowBackgroundConverter.FileName, ModelRowBackgroundConverter.GenerateSource(paired));
    }
}