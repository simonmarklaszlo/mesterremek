using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using SzivarClubManager.SourceGenerator.Changes.Generation;
using SzivarClubManager.SourceGenerator.Targets;
using SzivarClubManager.SourceGenerator.Targets.Factory;
using SzivarClubManager.SourceGenerator.Types;

namespace SzivarClubManager.SourceGenerator.Changes;

[Generator]
public class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var models = Model.GetCandidates(context);
        var factories = Factory.GetCandidates(context);
        var fakeFactories = FakeFactory.GetCandidates(context);

        var combined = models.Collect()
            .Combine(factories.Collect())
            .Combine(fakeFactories.Collect());

        context.RegisterSourceOutput(combined, static (spc, source) =>
        {
            var ((modelArray, factoryArray), fakeFactoryArray) = source;


            var factoryLookup = factoryArray
                .GroupBy(f => f.ModelSymbol, SymbolEqualityComparer.Default)
                .ToDictionary(g => g.Key, g => g.First(), SymbolEqualityComparer.Default);

            var fakeFactoryLookup = fakeFactoryArray
                .GroupBy(f => f.ModelSymbol, SymbolEqualityComparer.Default)
                .ToDictionary(g => g.Key, g => g.First(), SymbolEqualityComparer.Default);

            ModelGroup[] paired = new ModelGroup[modelArray.Length];

            for (var i = 0; i < modelArray.Length; i++)
            {
                var model = modelArray[i];
                if (!factoryLookup.TryGetValue(model.ModelSymbol, out var factory))
                {
                    spc.ReportDiagnostic(Diagnostic.Create(
                        Warnings.NoFactoryWarning,
                        model.ModelSymbol.Locations.FirstOrDefault(),
                        model.ModelSymbol.Name
                    ));
                }

                if (!fakeFactoryLookup.TryGetValue(model.ModelSymbol, out var fakeFactory))
                {
                    spc.ReportDiagnostic(Diagnostic.Create(
                        Warnings.NoFakeFactoryWarning,
                        model.ModelSymbol.Locations.FirstOrDefault(),
                        model.ModelSymbol.Name
                    ));
                }

                paired[i] = new ModelGroup(model, factory, fakeFactory);
            }

            spc.AddSource(Generation.Changes.FileName, Generation.Changes.GenerateSource(paired));
            spc.AddSource(ModelRowBackgroundConverter.FileName, ModelRowBackgroundConverter.GenerateSource(paired));
        });
    }
}